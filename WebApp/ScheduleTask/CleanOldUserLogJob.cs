using Quartz;
using WebApp.Services.LoggingService;

namespace WebApp.ScheduleTask;

public class CleanOldUserLogJob(ILogger<CleanOldUserLogJob> logger,
                                IServiceScopeFactory scopeFactory) : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Begin cleaning old log...");
        using var scope = scopeFactory.CreateScope();
        var time = DateTime.Now.AddDays(-60); // 60 days ago
        var userLogService = scope.ServiceProvider.GetRequiredService<IUserLogAppService>();
        var result = userLogService.ClearOutDateLog(time); // clear out date logs of 60 days ago
        logger.LogInformation("Clean old log success, count: {count}", result);
        return Task.CompletedTask;
    }
}