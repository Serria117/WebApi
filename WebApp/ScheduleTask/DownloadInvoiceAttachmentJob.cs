using Microsoft.EntityFrameworkCore;
using Quartz;
using WebApp.Core.DomainEntities;
using WebApp.Repositories;
using WebApp.Services.EmailService;

namespace WebApp.ScheduleTask;

[DisallowConcurrentExecution]
public class DownloadInvoiceAttachmentJob(IServiceScopeFactory scopeFactory,
                                          ILogger<DownloadInvoiceAttachmentJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var scope = scopeFactory.CreateScope();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailAppService>();
            var jobRepository = scope.ServiceProvider.GetRequiredService<IAppRepository<JobSetting, long>>();

            var jobSetting = await jobRepository.Find(j => j.Name == "DownloadInvoiceAttachmentJob")
                                                .FirstOrDefaultAsync();

            if (jobSetting == null || !jobSetting.IsActivated)
            {
                logger.LogInformation("DownloadInvoiceAttachmentJob is not activated. Skipping execution.");
                return;
            }

            logger.LogInformation("Begining download invoice attachments from email at {time}...",
                              DateTime.UtcNow.ToLocalTime().ToString("yy-MM-dd HH:mm:ss"));

            await emailService.AutoSyncEmailsAsync();

            logger.LogInformation("Finished download invoice attachments from email at {time}",
                                  DateTime.UtcNow.ToLocalTime().ToString("yy-MM-dd HH:mm:ss"));
        }
        catch
        {
            logger.LogError("Error occurred while downloading invoice attachments from email at {time}",
                            DateTime.UtcNow.ToLocalTime().ToString("yy-MM-dd HH:mm:ss"));
            throw; // Re-throw the exception to ensure Quartz handles it
        }
    }
}
