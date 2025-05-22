using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.OpenApi.Expressions;
using MimeKit;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services;

public class EmailService(IConfiguration config, 
                          IHostEnvironment env,
                          IUnitOfWork uow,
                          IUserManager userManager) : AppServiceBase(userManager)
{
    private readonly string _email = config["EmailSettings:Email"] ?? string.Empty;
    private readonly string _password = config["EmailSettings:AppPassword"] ?? string.Empty;
    
    
    public async Task<ICollection<MimeMessage>> FindEmailsAsync(EmailFilterRequest request)
    {
        if (_email.IsNullOrEmpty() || _password.IsNullOrEmpty())
        {
            throw new Exception("Email or password not configured");
        }

        using var client = new ImapClient();
        await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(_email, _password);
        var inbox = client.Inbox;
        await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);
        // 1. Search for emails from the sender
        SearchQuery query = SearchQuery.FromContains(request.Sender)
                                       .And(SearchQuery.BodyContains(request.Body));
        var uids = await inbox.SearchAsync(query);

        // 2. Filter emails by date range
        var messages = new List<MimeMessage>();
        foreach (var uid in uids)
        {
            var message = await inbox.GetMessageAsync(uid);
            if (message.Date.DateTime >= request.From && message.Date.DateTime <= request.To)
            {
                messages.Add(message);
            }
        }

        await client.DisconnectAsync(true);
        return messages;
    }

    public async Task SaveAttachmentsAsync(MimeMessage mail, string fileExtension)
    {
        if(WorkingOrg.IsNullOrEmpty()) throw new InvalidActionException("User has no organization");
        var saveDir = Path.Combine(env.ContentRootPath, "Downloads", "Attachments", WorkingOrg!);
        Directory.CreateDirectory(saveDir);
        foreach (var attachment in mail.Attachments)
        {
            if (attachment is not MimePart part) continue;

            if (!part.FileName.EndsWith(fileExtension)) continue;

            string fileName = Path.Combine(saveDir, part.FileName);
            await using var stream = File.Create(fileName);
            await part.Content.DecodeToAsync(stream);
        }
    }
}