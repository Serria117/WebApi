using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using WebApp.Mongo.MongoRepositories;
using WebApp.Services;
using WebApp.Services.CachingServices;
using WebApp.Services.UserService;

namespace WebApp.Authentication;

/// <summary>
/// Handles authorization requirements by validating permissions associated with authenticated user roles.
/// </summary>
public class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory,
                                            ILogger<PermissionAuthorizationHandler> log)
    : AuthorizationHandler<IAuthorizationRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                         IAuthorizationRequirement requirement)
    {
        var stringUserId = context.User.Claims
                                  .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(stringUserId, out var id))
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();
            //TODO: extract role from token and check if role is allowed to perform action
            var tokenRoles = context.User.Claims
                                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var username = context.User.Identity?.Name;

            var cacheService = scope.ServiceProvider.GetRequiredService<ICachingRoleService>();
            var lockService = scope.ServiceProvider.GetRequiredService<ILockedUserMongoRepository>();
            
            // Check if the user is locked
            try 
            {
                if (await lockService.IsUserLocked(id))
                {
                    log.LogWarning("AuthorizationHandler: " +
                                   "User [{username}] is locked. Access denied.", username);
                    context.Fail();
                    return;
                }
                log.LogInformation("AuthorizationHandler: " +
                                   "User [{username}] is not locked. Proceeding with authorization.", username);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "AuthorizationHandler: " +
                                 "Error checking lock status for user [{username}]. Authorization process rejected.", username);
                context.Fail();
                return;
            }
            
            if (tokenRoles is not null)
            {
                // Check if the user has the required permission in any of their roles
                var roles = tokenRoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var role in roles)
                {
                    var permissionsInRole = await cacheService.GetPermissionsFromCache(role);

                    switch (requirement)
                    {
                        case AllPermissionsRequirement allReq:
                            {
                                if (allReq.Permissions.All(p => permissionsInRole.Contains(p)))
                                {
                                    var permissions = string.Join(", ", allReq.Permissions);
                                    log.LogInformation("AuthorizationHandler: User [{username}] has role [{role}] " +
                                                       "contains all of the required permissions: [{permissions}] " +
                                                       "for the requested resource. " +
                                                       "Access granted!!", username, role, permissions);
                                    context.Succeed(requirement);
                                }
                                return;
                            }
                        case AnyPermissionRequirement anyReq:
                            {
                                if (anyReq.Permissions.Any(p => permissionsInRole.Contains(p)))
                                {
                                    var permissions = string.Join(", ", anyReq.Permissions);
                                    log.LogInformation("AuthorizationHandler: User [{username}] has role [{role}] " +
                                                       "contains one of the required permissions [{permissions}] for the requested resource. " +
                                                       "Access granted!!", username, role, permissions);
                                    context.Succeed(requirement);
                                }
                                return;
                            }
                        case PermissionRequirement rm when permissionsInRole.Contains(rm.Permission):
                            {
                                log.LogInformation("AuthorizationHandler: User [{username}] has role [{role}] " +
                                                   "contains the required permission [{permission}] " +
                                                   "for the requested resource. " +
                                                   "Access granted!!", username, role, rm.Permission);
                                context.Succeed(requirement);
                                return;
                            }
                    }
                }
            }
        }
    }
}