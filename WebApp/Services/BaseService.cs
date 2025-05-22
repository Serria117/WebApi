using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Salary;
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
}