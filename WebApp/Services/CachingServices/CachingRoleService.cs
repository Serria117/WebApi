using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Core.DomainEntities;
using WebApp.Repositories;

namespace WebApp.Services.CachingServices;

public interface ICachingRoleService
{
    Task<ISet<string>> GetPermissionsInRole(string roleName);
    void InvalidateCacheForRole(string roleName);
}

public class CachingRoleService(IMemoryCache cache, 
                                IServiceScopeFactory serviceScopeFactory,
                                ILogger<CachingRoleService> logger) : ICachingRoleService
{
    public async Task<ISet<string>> GetPermissionsInRole(string roleName) {
        var cacheKey = $"{roleName}_pemissions";
        
        // Check if the data exists in the cache.
        var permissions = await cache.GetOrCreateAsync<ISet<string>>(cacheKey, async entry =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var roleRepo = scope.ServiceProvider.GetRequiredService<IAppRepository<Role, int>>();
            var role = await roleRepo.Find(x => x.RoleName == roleName && !x.Deleted)
                                     .FirstOrDefaultAsync();
            if (role is null || role.Permissions.Count == 0) return new HashSet<string>(); // If found no role, return empty set.

            entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));
            entry.SetAbsoluteExpiration(TimeSpan.FromHours(1));
            
            return new HashSet<string>(role.Permissions.Select(p => p.PermissionName));

        });
        
        return permissions ?? new HashSet<string>(); // Return the cached value or an empty set.
    }
    
    public void InvalidateCacheForRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName)) return;

        string cacheKey = $"{roleName}_pemissions";
        cache.Remove(cacheKey);
        logger.LogInformation("Invalidated cache for key: {CacheKey}", cacheKey);
    }
}