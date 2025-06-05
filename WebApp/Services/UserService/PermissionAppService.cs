using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Mongo.MongoRepositories;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.Mappers;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.UserService
{
    public interface IPermissionAppService
    {
        Task<List<Permission>> GetPermissions();
        Task<List<string>> GetPermissionsFromMongo(Guid userId);
        Task<AppResponse> GetAllPermissionsInSystem();
        Task<List<MenuItemDisplayDto>> GetMenuItems();
    }

    public class PermissionBaseAppService(IAppRepository<User, Guid> userRepo,
                                      IAppRepository<Permission, int> permissionRepo,
                                      IAppRepository<Role, int> roleRepo,
                                      IAppRepository<MenuItem, int> menuItemRepo,
                                      IUserManager userManager,
                                      IUserMongoRepository userMongoRepository)
        : BaseAppService(userManager), IPermissionAppService
    {
        public async Task<List<Permission>> GetPermissions()
        {
            if (UserRoles is not { Count: > 0 }) return [];

            return await roleRepo.Find(r => UserRoles.Contains(r.RoleName))
                                 .Include(r => r.Permissions)
                                 .SelectMany(r => r.Permissions)
                                 .Distinct()
                                 .ToListAsync();
        }

        public async Task<List<MenuItemDisplayDto>> GetMenuItems()
        {
            var permissions = await GetPermissions();
            if (permissions.Count == 0) return []; //TODO: add default menu items (for guest users)

            var finalMenuItems = new List<MenuItem>();

            var hasPermisionMenu = await menuItemRepo.Find(filter: m => !m.Deleted &&
                                                                        m.MenuPermissions.Any(mp =>
                                                                                permissions.Contains(mp.Permission)))
                                                     .AsNoTracking()
                                                     .ToListAsync();

            var childrenMenu = hasPermisionMenu.Where(m => m.ParentId != null).ToList();

            // Fetch all potential parent IDs first
            var parentIds = hasPermisionMenu.Where(m => m.ParentId != null)
                                            .Select(m => m.ParentId!.Value)
                                            .Distinct()
                                            .ToList();

            // Fetch all parent items in a single query
            var parentItems = await menuItemRepo.Find(m => parentIds.Contains(m.Id))
                                                .AsNoTracking()
                                                .ToListAsync();

            // Create a lookup for faster access
            var parentItemsLookup = parentItems.ToDictionary(p => p.Id);

            // Group child items by their parent ID
            var childItemsByParentId = childrenMenu.GroupBy(m => m.ParentId!.Value);

            foreach (var group in childItemsByParentId)
            {
                var parentId = group.Key;

                // Add parent if it exists
                if (parentItemsLookup.TryGetValue(parentId, out var parentItem))
                {
                    finalMenuItems.Add(parentItem);
                    finalMenuItems.AddRange(group);
                }
            }

            return finalMenuItems.Select(m => new MenuItemDisplayDto
                                 {
                                     Id  = m.Id,
                                     Label = m.Label,
                                     Icon = m.Icon,
                                     To = m.To,
                                     ParentId = m.ParentId,
                                     Order = m.Order
                                 })
                                 .OrderBy(m => m.Order)
                                 .ToList();
        }

        public async Task<List<string>> GetPermissionsFromMongo(Guid userId)
        {
            return [.. (await userMongoRepository.GetUser(userId)).Permissions];
        }

        public async Task<AppResponse> GetAllPermissionsInSystem()
        {
            var permissions = await permissionRepo.Find(p => !p.Deleted)
                                                  .OrderBy(p => p.PermissionName)
                                                  .ToListAsync();
            //permissions.Select(mapper.Map<PermissionDisplayDto>)
            return AppResponse.OkResult(permissions.MapCollection(x => x.ToDisplayDto()));
        }
    }
}