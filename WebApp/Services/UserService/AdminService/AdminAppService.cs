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
    /// <returns>An <see cref="ResponseEntity"/> indicating the result of the operation, including success status and any additional information.</returns>
    Task<ResponseEntity> CreateMenu(MenuInputDto menu);

    /// <summary>
    /// Updates an existing menu item with the provided details.
    /// </summary>
    /// <param name="id">The unique identifier of the menu item to be updated.</param>
    /// <param name="input">An instance of <see cref="MenuInputDto"/> containing the updated details for the menu item.</param>
    /// <returns>An <see cref="ResponseEntity"/> indicating the result of the operation, including success status and any additional information.</returns>
    Task<ResponseEntity> UpdateMenu(int id, MenuInputDto input);

    /// <summary>
    /// Retrieves a menu item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the menu item to retrieve.</param>
    /// <returns>An <see cref="ResponseEntity"/> containing the menu item data if found, or an error response if the menu item does not exist.</returns>
    public Task<ResponseEntity> GetMenuById(int id);

    /// <summary>
    /// Sets specific permissions for a menu item identified by its ID.
    /// </summary>
    /// <param name="menuId">The unique identifier of the menu item for which the permissions are to be set.</param>
    /// <param name="permissionsId">A list of permission IDs to be associated with the menu item.</param>
    /// <returns>An <see cref="ResponseEntity"/> indicating the success or failure of the operation, with additional details if available.</returns>
    public Task<ResponseEntity> SetPermissionsForMenu(int menuId, List<int> permissionsId);

    /// <summary>
    /// Deletes a menu item with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the menu item to be deleted.</param>
    /// <returns>An <see cref="ResponseEntity"/> indicating the result of the operation, including success status and any additional information.</returns>
    Task<ResponseEntity> DeleteMenu(int id);

    /// <summary>
    /// Retrieves a list of all menu items available in the system.
    /// </summary>
    /// <returns>An <see cref="ResponseEntity"/> containing the list of menu items, including status and any additional information.</returns>
    Task<ResponseEntity> GetAllMenus();
}

public class AdminBaseAppService(IUserManager userManager,
                             IAppRepository<MenuItem, int> menuRepo,
                             IAppRepository<Permission, int> permissionRepo,
                             ILogger<AdminBaseAppService> logger) : BaseAppService(userManager), IAdminAppService
{
    public async Task<ResponseEntity> CreateMenu(MenuInputDto inputDto)
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
        return ResponseEntity.OkResult(result);
    }

    public async Task<ResponseEntity> UpdateMenu(int id, MenuInputDto input)
    {
        try
        {
            var found = await menuRepo.Find(x => x.Id == id)
                                      .Include(x => x.MenuPermissions)
                                      .FirstOrDefaultAsync();

            if (found is null) return ResponseEntity.Error404("Menu not found");

            //update menu with new values
            found.Label = input.Label;
            found.Icon = input.Icon;
            found.To = input.To;
            found.Order = input.Order;
            
            //remove old permissions
            found.MenuPermissions.Clear();
            
            /*foreach (var permission in found.MenuPermissions.ToList())
            {
                if (!input.Permissions.Contains(permission.PermissionId))
                {
                    found.MenuPermissions.Remove(permission);
                }
            }*/

            if (input.Permissions.Count <= 0)
            {
                return ResponseEntity.Ok();
            }

            var permissions = await permissionRepo.Find(x => input.Permissions.Contains(x.Id))
                                                  .Select(x => x.Id)
                                                  .ToListAsync();
            if (permissions.Count <= 0)
            {
                await menuRepo.UpdateAsync(found);
                return ResponseEntity.Ok();
            }

            foreach (var permission in permissions)
            {
                found.MenuPermissions.Add(new MenuPermission { PermissionId = permission });
            }

            await menuRepo.UpdateAsync(found);

            return ResponseEntity.Ok();
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}", e.Message);
            return ResponseEntity.Error500(e.Message);
        }
    }

    public async Task<ResponseEntity> GetMenuById(int id)
    {
        var found = await menuRepo.Find(x => x.Id == id)
                                  .Include(x => x.Parent)
                                  .Include(x => x.MenuPermissions)
                                  .FirstOrDefaultAsync();
        return found is null ? ResponseEntity.Error404("Menu not found") : ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> SetPermissionsForMenu(int menuId, List<int> permissionsId)
    {
        try
        {
            var menu = await menuRepo.Find(m => m.Id == menuId)
                                     .Include(m => m.MenuPermissions)
                                     .ThenInclude(mp => mp.Permission)
                                     .FirstOrDefaultAsync();
            if (menu is null) return ResponseEntity.Error404("Menu not found");

            // Remove all current permissions from the menu
            if (permissionsId.Count == 0)
            {
                menu.MenuPermissions.Clear();
                await menuRepo.UpdateAsync(menu);
                return ResponseEntity.Ok();
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
            return ResponseEntity.Ok();
        }
        catch (Exception e)
        {
            logger.LogError("Error: {messsage}", e.Message);
            logger.LogInformation("Stack trace: {stackTrace}", e.StackTrace);
            return ResponseEntity.Error(e.Message);
        }
    }

    public Task<ResponseEntity> DeleteMenu(int id)
    {
        throw new NotImplementedException();
    }

    public async Task<ResponseEntity> GetAllMenus()
    {
        try
        {
            var childList = await menuRepo.Find(x => x.Deleted == false && x.Parent == null)
                                          .Include(x => x.Items)
                                          .ThenInclude(x => x.MenuPermissions)
                                          .OrderBy(x => x.Order)
                                          .ToListAsync();
            return ResponseEntity.OkResult(childList);
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}", e.Message);
            return ResponseEntity.Error404(e.Message);
        }
    }
}