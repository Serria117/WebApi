using System.Diagnostics.Eventing.Reader;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Queues;
using WebApp.Repositories;
using WebApp.Services.UserService;

namespace WebApp.Services.LoggingService;

public interface IUserLogAppService
{
    Task ClearOutDateLog(DateTime date);
    Task CreateLog(string action, bool success, string? description = null);
    Task<bool> DeleteLog(Guid id);
    Task<UserLog?> GetLogById(Guid id);

    Task<List<UserLog>> GetLogs(string? userId = null, string? action = null, DateTime? fromDate = null,
                                DateTime? toDate = null);
}

public class UserLogAppService(IUserManager userManager,
                               IUserLogQueue logQueue,
                               IHttpContextAccessor context,
                               IAppRepository<UserLog, Guid> logRepository)
    : AppServiceBase(userManager), IUserLogAppService
{
    public Task CreateLog(string action, bool success = true, string? description = null)
    {
        var log = new UserLog
        {
            Action = action,
            Description = description,
            ActionTime = DateTime.UtcNow.ToLocalTime(),
            UserId = UserId == null ? null : Guid.Parse(UserId!),
            UserName = UserName,
            Success = success,
            Ip = context.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP Address",
        };
        logQueue.Enqueue(log);
        return Task.CompletedTask;
        //await logRepository.CreateAsync(log);
    }

    public async Task<List<UserLog>> GetLogs(string? userId = null, 
                                             string? action = null,
                                             DateTime? fromDate = null, 
                                             DateTime? toDate = null)
    {
        var query = logRepository.GetQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(x => x.UserId.ToString() == userId);
        }

        if (!string.IsNullOrEmpty(action))
        {
            query = query.Where(x => x.Action == action);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.ActionTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.ActionTime <= toDate.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<UserLog?> GetLogById(Guid id)
    {
        return await logRepository.FindByIdAsync(id);
    }

    public async Task<bool> DeleteLog(Guid id)
    {
        return await logRepository.HardDeleteAsync(id);
    }

    public async Task ClearOutDateLog(DateTime date)
    {
        var logs = await logRepository.Find(x => x.ActionTime < date)
                                      .Select(x => x.Id).ToListAsync();
        await logRepository.HardDeleteManyAsync(logs);
    }
}