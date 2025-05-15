using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WebApp.Services.UserService;

public interface IUserManager
{
    string? CurrentUsername();
    string? CurrentUserId();
    string? WorkingOrg();
    List<string> GetRoles();
}

public class UserManager(IHttpContextAccessor httpContextAccessor) : IUserManager
{
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    public string? CurrentUsername()
    {
        return _httpContext?.User.Identity?.Name;
    }

    public string? CurrentUserId()
    {
        const string claimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        return _httpContext?.User.Claims.FirstOrDefault(x => x.Type == claimType)?.Value;
    }

    //TODO: add tenant, working organizations, etc...

    public string? WorkingOrg()
    {
        var orgId = _httpContext?.User.Claims.FirstOrDefault(x => x.Type == "orgId")?.Value;
        return orgId;
    }

    public List<string> GetRoles()
    {
        var roles = new List<string>();
        const string roleClaimType = ClaimTypes.Role;
        var roleClaims = _httpContext?.User.Claims.FirstOrDefault(x => x.Type == roleClaimType);
        if (roleClaims is not null && !string.IsNullOrEmpty(roleClaims.Value))
        {
            roles.AddRange(roleClaims.Value.Split(","));
        }

        return roles;
    }
}