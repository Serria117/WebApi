using Quartz;
using WebApp.Mongo.MongoRepositories;

namespace WebApp.ScheduleTask;

public class ClearOldRefreshTokenJob(IRefreshTokenMongoRepository refreshTokenMongoRepository,
                                     ILogger<ClearOldRefreshTokenJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Clear old refresh token job started at {date}...", 
                              DateTime.UtcNow.ToLocalTime().ToString("yy-MM-dd HH:mm:ss"));
        
        var deleteCount = await refreshTokenMongoRepository.DeleteOldToken();
        
        logger.LogInformation("Clear old refresh token job finished at {date}. Deleted {count} token(s).", 
                              DateTime.UtcNow.ToLocalTime().ToString("yy-MM-dd HH:mm:ss"), deleteCount);
    }
}