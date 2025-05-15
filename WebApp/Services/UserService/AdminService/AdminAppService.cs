using Microsoft.EntityFrameworkCore;
using WebApp.Core;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.UserService.Dto;

namespace WebApp.Services.UserService.AdminService;

/// <summary>
/// Provides an interface for managing administrative operations related to menu items.
/// </summary>
public interface IAdminAppService
{
    /// <summary>
    /// Creates a new menu item based on the provided details.
    /// </summary>
    /// <param name="menu">An instance of <see cref="MenuInputDto"/> containing the details of the menu item to be created.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation, including success status and any additional information.</returns>
    Task<AppResponse> CreateMenu(MenuInputDto menu);

    /// <summary>
    /// Updates an existing menu item with the provided details.
    /// </summary>
    /// <param name="id">The unique identifier of the menu item to be updated.</param>
    /// <param name="menu">An instance of <see cref="MenuInputDto"/> containing the updated details for the menu item.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation, including success status and any additional information.</returns>
    Task<AppResponse> UpdateMenu(int id, MenuInputDto menu);

    public Task<AppResponse> GetMenuById(int id);

    /// <summary>
    /// Sets specific permissions for a menu item identified by its ID.
    /// </summary>
    /// <param name="menuId">The unique identifier of the menu item for which the permissions are to be set.</param>
    /// <param name="permissionsId">A list of permission IDs to be associated with the menu item.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the success or failure of the operation, with additional details if available.</returns>
    public Task<AppResponse> SetPermissionsForMenu(int menuId, List<int> permissionsId);

    /// <summary>
    /// Deletes a menu item with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the menu item to be deleted.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation, including success status and any additional information.</returns>
    Task<AppResponse> DeleteMenu(int id);

    /// <summary>
    /// Retrieves a list of all menu items available in the system.
    /// </summary>
    /// <returns>An <see cref="AppResponse"/> containing the list of menu items, including status and any additional information.</returns>
    Task<AppResponse> GetAllMenus();
}

public class AdminAppService(IUserManager userManager,
                             IAppRepository<MenuItem, int> menuRepo,
                             IAppRepository<Role, int> roleRepo,
                             IAppRepository<Permission, int> permissionRepo,
                             IPermissionAppService permissionAppService,
                             ILogger<AdminAppService> logger,
                             IRoleAppService roleAppService) : AppServiceBase(userManager), IAdminAppService
{
    public async Task<AppResponse> CreateMenu(MenuInputDto inputDto)
    {
        var permissions = await permissionRepo.Find(x => inputDto.Permissions.Contains(x.Id))
                                              .ToListAsync();
        var menu = new MenuItem()
        {
            Label = inputDto.Label,
            Icon = inputDto.Icon,
            To = inputDto.To,
            Order = inputDto.Order,
            ParentId = inputDto.ParentId,
        };
        foreach (var permission in permissions)
            menu.MenuPermissions.Add(new MenuPermission { PermissionId = permission.Id });

        var result = await menuRepo.CreateAsync(menu);
        return AppResponse.SuccessResponse(result);
    }

    public async Task<AppResponse> UpdateMenu(int id, MenuInputDto menu)
    {
        var found = await menuRepo.Find(x => x.Id == id)
                                  .Include(x => x.MenuPermissions)
                                  .FirstOrDefaultAsync();
        
        if (found is null) return AppResponse.Error404("Menu not found");
        
        //TODO: update the found object with the new data
        
        return AppResponse.Ok();
    }

    public async Task<AppResponse> GetMenuById(int id)
    {
        var found = await menuRepo.Find(x => x.Id == id)
                                  .Include(x => x.Parent)
                                  .Include(x => x.MenuPermissions)
                                  .FirstOrDefaultAsync();
        return found is null ? AppResponse.Error404("Menu not found") : AppResponse.SuccessResponse(found);
    }

    public async Task<AppResponse> SetPermissionsForMenu(int menuId, List<int> permissionsId)
    {
        try
        {
            var menu = await menuRepo.Find(m => m.Id == menuId)
                                     .Include(m => m.MenuPermissions)
                                     .ThenInclude(mp => mp.Permission)
                                     .FirstOrDefaultAsync();
            if (menu is null) return AppResponse.Error404("Menu not found");

            // Remove all current permissions from the menu
            if (permissionsId.Count == 0)
            {
                menu.MenuPermissions.Clear();
                await menuRepo.UpdateAsync(menu);
                return AppResponse.Ok();
            }

            var permissionsToRemove = menu.MenuPermissions.Where(mp => !permissionsId.Contains(mp.PermissionId))
                                          .ToList();

            foreach (var permission in permissionsToRemove)
                menu.MenuPermissions.Remove(permission);

            var permissionsToAdd = await permissionRepo.Find(p => permissionsId.Contains(p.Id))
                                                       .ToListAsync();
            foreach (Permission permission in permissionsToAdd)
            {
                menu.MenuPermissions.Add(new MenuPermission
                {
                    PermissionId = permission.Id,
                });
            }

            await menuRepo.UpdateAsync(menu);
            return AppResponse.Ok();
        }
        catch (Exception e)
        {
            logger.LogError("Error: {messsage}", e.Message);
            logger.LogInformation("Stack trace: {stackTrace}", e.StackTrace);
            return AppResponse.Error(e.Message);
        }
    }

    public Task<AppResponse> DeleteMenu(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<AppResponse> GetAllMenus()
    {
        try
        {
            var childList = await menuRepo.Find(x => x.Deleted == false && x.Parent == null)
                                          .Include(x => x.Items)
                                          .ThenInclude(x => x.MenuPermissions)
                                          .OrderBy(x => x.Order)
                                          .ToListAsync();
            return AppResponse.SuccessResponse(childList);
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}", e.Message);
            return AppResponse.Error404(e.Message);
        }
    }
}