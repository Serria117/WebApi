using System.Linq.Expressions;
using System.Security.Cryptography;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using WebApp.Core.DomainEntities;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.EmailService;

public interface IEmailAppService
{
    /// <summary>
    /// Sync all emails' invoice attachment with the current email configs
    /// </summary>
    /// <returns></returns>
    Task AutoSyncEmailsAsync();

    Task<ResponseEntity> FindEmailsAsync(EmailFilterRequest request);
    Task SendEmailAsync(string subject, string body, string recipient);
}

public class EmailAppService(IConfiguration config,
                             IHostEnvironment env,
                             ILogger<EmailAppService> logger,
                             IAppRepository<EmailSenderAddress, long> senderRepository,
                             IAppRepository<EmailAttachment, long> attachmentRepository,
                             IAppRepository<EmailConfig, int> emailConfigRepository,
                             IUserManager userManager) : BaseAppService(userManager), IEmailAppService
{
    public async Task SendEmailAsync(string subject, string body, string recipient)
    {
        var adminEmail = "ketoan.sline@gmail.com";
        var emailConfig = await emailConfigRepository.Find(x => x.Email == adminEmail)
                                                     .FirstOrDefaultAsync();
        if (emailConfig is null) throw new NotFoundException("Email configuration not found");
        if (string.IsNullOrEmpty(body)) throw new InvalidActionException("Email body cannot be empty");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress("Sline admin", emailConfig.Email));
        message.To.Add(new MailboxAddress("", recipient));
        message.Subject = subject;
        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = body
        };
        message.Body = bodyBuilder.ToMessageBody();
        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(emailConfig!.Email, emailConfig.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task AutoSyncEmailsAsync()
    {
        var senders = await senderRepository.Find(s => s.Deleted == false)
                                            .ToListAsync();
        var emailsConfig = await emailConfigRepository.Find(e => e.Deleted == false)
                                                      .ToListAsync();

        if (emailsConfig.Count == 0 || senders.Count == 0) return;
        foreach (var config in emailsConfig)
        {
            foreach (var sender in senders)
            {
                try
                {
                    var request = new EmailFilterRequest
                    {
                        Email = config.Email,
                        Sender = sender.Email,
                        Subject = "Hóa đơn",
                        From = DateTime.Now.AddDays(-1),
                        To = DateTime.Now,
                        FileType = "xml"
                    };
                    var response = await FindEmailsAsync(request);
                    if (response.Success)
                    {
                        logger.LogInformation($"Successfully synced emails for {sender.Name}");
                    }
                    else
                    {
                        logger.LogWarning($"Failed to sync emails for {sender.Name}: {response.Message}");
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error syncing emails for {sender.Name}");
                }
            }
        }
    }

    public async Task<ResponseEntity> FindEmailsAsync(EmailFilterRequest request)
    {
        try
        {
            var emailConfig = await emailConfigRepository.Find(x => x.Email == request.Email)
                                                         .FirstOrDefaultAsync();

            NotFoundException.ThrowIfNull(emailConfig, "Email configuration not matched or not found.");

            using var client = new ImapClient();
            await client.ConnectAsync("imap.gmail.com", 993, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(emailConfig!.Email, emailConfig.AppPassword);
            var inbox = client.Inbox;
            await inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);
            // 1. Search for emailsConfig from the sender
            SearchQuery query = SearchQuery.FromContains(request.Sender);
            if (request.Subject is not null)
            {
                query = query.And(SearchQuery.SubjectContains(request.Subject));
            }

            if (request.Body is not null)
            {
                query = query.And(SearchQuery.BodyContains(request.Body));
            }

            //filter by date:
            query = query.And(SearchQuery.DeliveredAfter(request.From?.AddDays(-1) ?? new DateTime(2020, 1, 1))
                                         .And(SearchQuery.DeliveredBefore(request.To?.AddDays(1) ?? DateTime.Now)));

            var uids = await inbox.SearchAsync(query);

            var textBody = new List<string>();
            var attachmentCount = 0;
            foreach (var uid in uids)
            {
                var message = await inbox.GetMessageAsync(uid);
                attachmentCount += await SaveAttachmentsAsync(message, request.FileType ?? "xml");
            }

            await client.DisconnectAsync(true);
            return new ResponseEntity
            {
                Success = true,
                Data = new
                {
                    TotalEmails = uids.Count,
                    Message = "Emails loaded successfully.",
                    AttachmentsSaved = attachmentCount
                }
            };
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return ResponseEntity.Error("Lỗi khi tải email!");
        }
    }

    private async Task<int> SaveAttachmentsAsync(MimeMessage message, string fileExtension)
    {
        if (WorkingOrg.ToGuid() == Guid.Empty) throw new InvalidActionException("User has no organization");
        var from = message.From.Mailboxes.FirstOrDefault();
        var fromName = from?.Address ?? "NON_SENDER_NAME";

        var saveDir = Path.Combine(env.ContentRootPath, "Downloads", "Attachments",
                                   WorkingOrg!, fromName);
        Directory.CreateDirectory(saveDir);
        var countAttachment = 0;
        foreach (var attachment in message.Attachments)
        {
            if (attachment is not MimePart part) continue;

            if (!part.FileName.EndsWith(fileExtension)) continue;

            using var memoryStream = new MemoryStream();
            await part.Content.DecodeToAsync(memoryStream);
            var contentHash = ComputeHash(memoryStream.ToArray());
            if (await IsEmailDownloaded(message.MessageId, contentHash))
            {
                logger.LogInformation($"Attachment {part.FileName} already downloaded for message {message.MessageId}");
                continue;
            }

            string fileName = Path.Combine(saveDir, $"{message.MessageId}-F{countAttachment}.{fileExtension}");
            await using var stream = File.Create(fileName);
            await part.Content.DecodeToAsync(stream);

            await attachmentRepository.CreateAsync(new EmailAttachment
            {
                Email = fromName,
                MessageId = message.MessageId,
                FileName = fileName,
                HashValue = contentHash,
                OrganizationId = WorkingOrg.ToGuid()
            });
            countAttachment++;
        }

        return countAttachment;
    }

    private async Task<bool> IsEmailDownloaded(string messageId, string hashValue)
    {
        return await attachmentRepository.ExistAsync(e => e.HashValue == hashValue);
    }

    private static string ComputeHash(byte[] data)
    {
        var hashBytes = SHA256.HashData(data);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}