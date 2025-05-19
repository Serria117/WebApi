using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace WebApp.Authentication;

public class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);
        if (policyName.StartsWith("HasAll:"))
        {
            var permissions = policyName["HasAll:".Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            return new AuthorizationPolicyBuilder()
                   .AddRequirements(new AllPermissionsRequirement(permissions))
                   .Build();
        }
        
        if (policyName.StartsWith("HasAny:"))
        {
            var permissions = policyName["HasAny:".Length..].Split(',', StringSplitOptions.RemoveEmptyEntries);
            return new AuthorizationPolicyBuilder()
                   .AddRequirements(new AnyPermissionRequirement(permissions))
                   .Build();
        }
        
        if (policy is not null)
        {
            return policy;
        }

        return new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();
    }
}
