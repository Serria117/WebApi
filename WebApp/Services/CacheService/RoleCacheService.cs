using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Core.DomainEntities;
using WebApp.Repositories;
using WebApp.Services.UserService;

namespace WebApp.Services.CacheService;

public interface IRoleCacheService
{
    Task<ISet<string>> GetPermisionsAsync(string roleName);
    void InvalidateCache(string roleName);
}

public class RoleCacheService(IMemoryCache memoryCache,
                              IAppRepository<Role, int> roleRepository,
                              ILogger<RoleCacheService> logger) : IRoleCacheService
{
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public async Task<ISet<string>> GetPermisionsAsync(string roleName)
    {
        var cacheKey = $"{roleName}_permissions";
        var permissions = await memoryCache.GetOrCreateAsync<HashSet<string>>(cacheKey, async entry =>
            {
                logger.LogInformation("Getting permissions from database...");
                var role = await roleRepository.Find(filter: r => r.RoleName == roleName && !r.Deleted, 
                                                     include: [nameof(Role.Permissions)])
                                               .FirstOrDefaultAsync();
                var permissionsFromDb = new List<string>();
                if (role is not null)
                {
                    permissionsFromDb = role.Permissions.Where(p => !p.Deleted)
                                            .Select(p => p.PermissionName)
                                            .ToList();
                    logger.LogInformation($"Permissions retrieved: {string.Join(", ", permissionsFromDb)}");
                }

                entry.AbsoluteExpirationRelativeToNow = _cacheDuration; // Set the expiration time for the cached data
                return new HashSet<string>(permissionsFromDb, StringComparer.OrdinalIgnoreCase);
            }
        );
        return permissions ?? [];
    }

    public void InvalidateCache(string roleName)
    {
        memoryCache.Remove($"{roleName}_permissions"); // Remove all permission caches
    }
}