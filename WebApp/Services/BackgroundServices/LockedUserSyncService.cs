using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.MongoRepositories;
using WebApp.Repositories;

namespace WebApp.Services.BackgroundServices;

public class LockedUserSyncService(IServiceScopeFactory factory,
                                   ILogger<LockedUserSyncService> log): BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(48); //Run every 48 hours
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            log.LogInformation("LockedUserSyncService is running at: {time}", DateTimeOffset.Now);
            using var serviceScope = factory.CreateScope();
            var userMongoRepository = serviceScope.ServiceProvider.GetRequiredService<ILockedUserMongoRepository>();
            var userRepository = serviceScope.ServiceProvider.GetRequiredService<IAppRepository<User, Guid>>();
            try
            {
                var lockedUserInMongo = await userMongoRepository.GetAllLockedUserIds(stoppingToken);
                var lockedUserInDb = await userRepository.Find(x => x.Locked == true)
                                                         .Select(x => x.Id)
                                                         .ToListAsync(stoppingToken);
                ;
                var lockedUserToDelete = lockedUserInDb.Except(lockedUserInMongo).ToList();
                var lockedUserToAdd = lockedUserInMongo.Except(lockedUserInDb).ToList();

                foreach (var userId in lockedUserToDelete)
                {
                    await userMongoRepository.UnlockUser(userId);
                    log.LogInformation("Unlocked user [{userId}]", userId);
                }

                foreach (var userId in lockedUserToAdd)
                {
                    await userMongoRepository.LockUser(userId);
                    log.LogInformation("Locked user [{userId}]", userId);
                }

                log.LogInformation("LockedUserSyncService completed at: {time}", DateTimeOffset.Now);
            }
            catch (Exception e)
            {
                log.LogError(e, "Error during LockedUserSyncService execution: {message}", e.Message);
            }
            await Task.Delay(_interval, stoppingToken);
        }
        
        
    }
}