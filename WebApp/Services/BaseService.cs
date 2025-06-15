using WebApp.Services.UserService;

namespace WebApp.Services;

/// <summary>
/// Base service class providing common user-related properties.
/// </summary>
/// <param name="userManager">The user manager instance to retrieve user information.</param>
public class BaseAppService(IUserManager userManager)
{
    /// <summary>
    /// Gets the user manager instance.
    /// </summary>
    protected IUserManager UserManager { get; } = userManager;

    /// <summary>
    /// Gets the current user's ID.
    /// </summary>
    protected string? UserId { get; } = userManager.CurrentUserId();
    
    /// <summary>
    /// Gets the current user's username.
    /// </summary>
    protected string? UserName { get; } = userManager.CurrentUsername();
    
    /// <summary>
    /// Gets the current working organization of the user.
    /// </summary>
    protected string? WorkingOrg { get; } = userManager.WorkingOrg();

    /// <summary>
    /// Gets the list of roles assigned to the current user.
    /// </summary>
    protected List<string> UserRoles { get; } = userManager.GetRoles();
}