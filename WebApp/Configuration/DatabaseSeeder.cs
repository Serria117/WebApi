using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Enums;
using WebApp.Services.CachingServices;

namespace WebApp.Configuration;

public class DatabaseSeeder(AppDbContext context, ICachingRoleService caching)
{
    /// <summary>
    /// Seeds the database with default data.
    /// </summary>
    public async Task SeedAsync()
    {
        Console.WriteLine("Performing database seeding...");
        await SeedPermissions();
        await SeedAdminRole();
        await PreLoadCachingRoles();
        //await SeedPayrollComponentCategory();
        //await SeedGeneralPayrollInputType();
        Console.WriteLine("Finished seeding. Application is ready to use.");
    }

    //Seed default permissions
    private async Task SeedPermissions()
    {
        Console.WriteLine("--- Seeding default permissions...");
        var existingPermissions = context.Permissions.Select(p => p.PermissionName).ToHashSet();
        var defaultPermissions = PermissionSeeder.GetDefaultPermissions();
        var permissionsToAdd = defaultPermissions.Where(permission => !existingPermissions.Contains(permission))
                                                 .Select(permission => new Permission { PermissionName = permission })
                                                 .ToList();
        if (!permissionsToAdd.IsNullOrEmpty())
        {
            await context.AddRangeAsync(permissionsToAdd);
            await context.SaveChangesAsync();
            Console.WriteLine($"------ {permissionsToAdd.Count} permissions added.");
        }

        Console.WriteLine("------ All permissions are up-to-date. No new permissions added.");
    }

    private async Task SeedAdminRole()
    {
        Console.WriteLine("--- Seeding Admin role...");
        var roleAdmin = await context.Roles.Include(r => r.Permissions)
                                     .FirstOrDefaultAsync(r => r.RoleName == "Admin");
        if (roleAdmin == null)
        {
            roleAdmin = new Role { RoleName = "Admin" };
            await context.Roles.AddAsync(roleAdmin);
            await context.SaveChangesAsync();
        }

        var existingPermissions = roleAdmin.Permissions.Select(p => p.PermissionName).ToHashSet();
        var defaultPermissions = PermissionSeeder.GetDefaultPermissions();
        var newPermissionName = defaultPermissions.Where(permission => !existingPermissions.Contains(permission))
                                                  .ToList();
        if (newPermissionName.IsNullOrEmpty()) return; // No new permissions to add
        
        // Fetching permissions to add based on the new permission names
        var permissionsToAdd = await context.Permissions
                                            .Where(p => newPermissionName.Contains(p.PermissionName))
                                            .ToListAsync();
        
        if (!permissionsToAdd.IsNullOrEmpty()) return; // No new permissions to add
        
        foreach (var permission in permissionsToAdd)
        {
            if (roleAdmin.Permissions.All(p => p.PermissionName != permission.PermissionName))
            {
                roleAdmin.Permissions.Add(permission);
            }
        }

        await context.SaveChangesAsync();
    }

    //Preload all permission of each role into memory cache for fast authorization check
    private async Task PreLoadCachingRoles()
    {
        Console.WriteLine("--- Loading role into cache for faster role-check during authorization processing...");
        var roles = await context.Roles.Select(r => r.RoleName).ToListAsync();
        foreach (var role in roles)
        {
            await caching.GetPermissionsFromCache(role);
        }
    }
}