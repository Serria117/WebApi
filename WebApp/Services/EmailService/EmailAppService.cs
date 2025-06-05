using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using WebApp.Core.DomainEntities;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.EmailService;

public class EmailService(IConfiguration config, 
                          IHostEnvironment env,
                          IAppRepository<EmailConfig, int> emailConfigRepository,
                          IUserManager userManager) : BaseAppService(userManager)
{
    
    public async Task<ICollection<MimeMessage>> FindEmailsAsync(EmailFilterRequest request)
    {
        var emailConfig = await emailConfigRepository.Find(x => x.Email == request.Email)
                                                     .FirstOrDefaultAsync();
        
        NotFoundException.ThrowIfNull(emailConfig, "Email configuration not matched or not found.");
        
        using var client = new ImapClient();
        await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(emailConfig!.Email, emailConfig.AppPassword);
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