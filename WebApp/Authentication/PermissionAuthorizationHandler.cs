using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebApp.Services;
using WebApp.Services.CachingServices;
using WebApp.Services.UserService;

namespace WebApp.Authentication;

public class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory,
                                            ILogger<PermissionAuthorizationHandler> log) 
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                         PermissionRequirement requirement)
    {
        var stringUserId = context.User.Claims
                                  .FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(stringUserId, out var id))
        {
            // Extract permissions from token
            /*var tokenPermissions = context.User.Claims
                .FirstOrDefault(c => c.Type == "permissions")?.Value
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .ToList() ?? new List<string>();*/

            // Check if token permissions contain the required permission
            /*if (tokenPermissions.Contains(requirement.Permission))
            {
                log.LogInformation("Found permisions claim on token. Checking if it has the required permission...");
                context.Succeed(requirement);
                return;
            }*/
            
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            
            // Add this before checking for claim name:
            /*foreach (var claim in context.User.Claims)
            {
                log.LogInformation("Claim Type: {ClaimType}, Claim Value: {ClaimValue}", 
                                   claim.Type, claim.Value);
            }
            */
        
            
            //TODO: extract role from token and check if role is allowed to perform action
            var tokenRoles = context.User.Claims
                                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var cacheService = scope.ServiceProvider.GetRequiredService<ICachingRoleService>();
            
            if (tokenRoles is not null)
            {
                var roles = tokenRoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach(var role in roles)
                {
                    var permissionsInRole = await cacheService.GetPermissionsInRole(role);
                    
                    if (permissionsInRole.Contains(requirement.Permission))
                    {
                        log.LogInformation("User's role [{role}] contains the required permission [{pemission}]. " +
                                           "Succeeded authorization!", role, requirement.Permission);
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
        }
    }
}