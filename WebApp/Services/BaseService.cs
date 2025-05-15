using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;

namespace WebApp.Services;

public class AppServiceBase(IUserManager userManager)
{
    protected IUserManager UserManager { get; } = userManager;
    /// <summary>
    /// Get the current user's id
    /// </summary>
    protected string? UserId { get; } = userManager.CurrentUserId();
    
    /// <summary>
    /// Get current username
    /// </summary>
    protected string? UserName { get; } = userManager.CurrentUsername();
    
    /// <summary>
    /// Get current working organization of current user
    /// </summary>
    protected string? WorkingOrg { get; } = userManager.WorkingOrg();

    /// <summary>
    /// Get the list of roles assigned to the current user
    /// </summary>
    protected List<string> UserRoles { get; } = userManager.GetRoles();

    /// <summary>
    /// Extract Guid from a string
    /// </summary>
    /// <param name="id">The string represent a Guid</param>
    /// <returns>true and the Guid instance if the string was successfully converted to Guid, or failed if not</returns>
    protected static (bool Result, Guid Id) GetId(string? id)
    {
        if (string.IsNullOrEmpty(id) || !Guid.TryParse(id, out var parsedId))
        {
            return (false, Guid.Empty);
        }

        return (true, parsedId);
    }
}