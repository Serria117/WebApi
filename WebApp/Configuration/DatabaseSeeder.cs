using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Salary;
using WebApp.Enums;
using WebApp.Enums.Payroll;
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
        await SeedPayrollComponentCategory();
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
        var permissionsToAdd = defaultPermissions.Where(permission => !existingPermissions.Contains(permission))
                                                 .Select(permission => new Permission { PermissionName = permission })
                                                 .ToList();

        //var permissions = await context.Permissions.ToListAsync();
        if (permissionsToAdd.IsNullOrEmpty()) return; // No new permissions to add

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

    private async Task SeedPayrollComponentCategory()
    {
        Console.WriteLine("--- Seeding Payroll Component Category...");
        var existingCategories = context.PayrollComponentCategories.Select(c => c.Name).ToHashSet();
        var defaultCategories = ComponentCategoryType.GetFields().ToList();
        var categoriesToAdd = defaultCategories
                              .Where(category => category != null && !existingCategories.Contains(category))
                              .Select(category => new PayrollComponentCategory
                              {
                                  Name = category ?? string.Empty,
                                  Description = category switch
                                  {
                                      ComponentCategoryType.TaxableIncome => "Các khoản thu nhập tính vào thu nhập chịu thuế",
                                      ComponentCategoryType.NonTaxableIncome => "Các khoản thu nhập không tính vào chịu thuế",
                                      ComponentCategoryType.Deduction => "Các khoản phải khấu trừ",
                                      ComponentCategoryType.PersonalIncomeTax => "Thuế TNCN",
                                      _ => null
                                  },
                                  Order = defaultCategories.IndexOf(category) + 1,
                              })
                              .ToList();
        if(categoriesToAdd.IsNullOrEmpty()) return;
        await context.PayrollComponentCategories.AddRangeAsync(categoriesToAdd);
        await context.SaveChangesAsync();
    }

    private async Task SeedGeneralPayrollInputType()
    {
        Console.WriteLine("--- Seeding General Payroll Input Type...");
        var existingTypes = context.PayrollInputTypes
                                   .Where(t => t.OrganizationId == null)
                                   .Select(t => t.Name).ToHashSet();
        var defaultTypes = new List<PayrollInputType>
        {
            new() { Name = "Số người phụ thuộc", Unit = "Người", DataType = InputDataType.Number },
            new() { Name = "Ngày công", Unit = "Ngày", DataType = InputDataType.Date },
            new() { Name = "Ngày nghỉ", Unit = "Ngày", DataType = InputDataType.Date },
            new() { Name = "Ngày đi công tác", Unit = "Ngày", DataType = InputDataType.Date },
        };
        var typesToAdd = defaultTypes.Where(type => !existingTypes.Contains(type.Name))
                                     .ToList();
        if (!typesToAdd.IsNullOrEmpty())
            await context.PayrollInputTypes.AddRangeAsync(typesToAdd);
    }
}