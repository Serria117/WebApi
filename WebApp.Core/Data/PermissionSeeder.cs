using System.Reflection;
using WebApp.Core.DomainEntities;
using WebApp.Enums;

namespace WebApp.Core.Data;

public static class PermissionSeeder
{
    public static List<string> GetDefaultPermissions()
    {
        // Use reflection to get all fields in the Permissions struct
        return typeof(Permissions)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Select(field => field.GetValue(null).ToString())
            .ToList();
    }
}