using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using WebApp.Payloads;
using WebApp.Utils;

namespace WebApp.Services.CachingServices;

public static class CacheKeyBuilder
{
    public static string ForUserOrganizations(Guid userId, PageRequest req)
    {
        var parts = new[]
        {
            "orgs",
            $"user:{userId}",
            $"kw:{req.Keyword.RemoveSpace()?.UnSign() ?? "null"}",
            $"p:{req.Page}",
            $"s:{req.Size}",
            $"sort:{req.SortBy}",
            $"order:{req.OrderBy}",
            req.Fields.Length > 0 ? $"f:{string.Join(",", req.Fields.OrderBy(f => f))}" : ""
        };
        return string.Join("|", parts);
    }
    
    // Helper tạo version prefix
    public static string UserOrgVersionPrefix(Guid userId) => $"org_version:u:{userId}";
    
    public static string GlobalOrgVersionPrefix => "org_version:global";
    
    public static async Task<string> WithVersion(this string key, 
                                                 IDistributedCache cache, 
                                                 string versionKey = "global")
    {
        var version = await cache.GetStringAsync($"cache_version:{versionKey}") ?? "v1";
        return $"{key}|{version}";
    }

    public static string ByUserRequest(IHttpContextAccessor contextAccessor)
    {
        var context = contextAccessor.HttpContext;
        if (context is null) return string.Empty;
        
        var request = context.Request;
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value.ToGuid();
        List<string> keyParts = [
            $"user: {userId}", $"path: {request.Path}", $"method: {request.Method}"
        ];
        foreach (var (key, value) in request.Query.OrderBy(q => q.Key))
        {
            keyParts.Add($"q_{key}:{value}");
        }
        return  string.Join("|", keyParts);
    }
}