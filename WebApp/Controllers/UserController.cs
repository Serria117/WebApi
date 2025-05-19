using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/user")][Authorize]
public class UserController(IUserAppService userAppService) : ControllerBase
{
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="dto">Input data</param>
    /// <returns>Created user</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserCreate"/> permission.
    /// </remarks>
    [HttpPost("create"), HasAllAuthorities(Permissions.UserCreate, Permissions.Admin)]
    public async Task<IActionResult> CreateUser(UserInputDto dto)
    {
        try
        {
            var res = await userAppService.CreateUser(dto);
            return Ok(res);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Retrieve a paginated list of all users
    /// </summary>
    /// <param name="req">The request parameters for pagination and filtering</param>
    /// <returns>A paginated list of users or an error message if the parameters are invalid</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserView"/> permission.
    /// </remarks>
    [HttpGet("all")] [HasAuthority(Permissions.UserView)]
    public async Task<IActionResult> GetAll([FromQuery] RequestParam req)
    {
        try
        {
            var pagedRequest = PageRequest.BuildRequest(req);
            var res = await userAppService.GetAllUsers(pagedRequest);
            return res.Success ? Ok(res) : BadRequest(res);
        }
        catch (Exception)
        {
            return BadRequest(ErrorResponse.InvalidParams());
        }
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The details of the user if found, or an error response if not.</returns>
    /// <remarks>
    /// Requires either the <see cref="Permissions.UserSelf"/> or <see cref="Permissions.UserView"/> permission.
    /// </remarks>
    [HttpGet("{userId:guid}")]
    [HasAnyAuthority(Permissions.UserSelf, Permissions.UserView)]
    public async Task<IActionResult> GetById(Guid userId)
    {
        var res = await userAppService.FindUserById(userId);
        return res.Success ? Ok(res) : BadRequest(res);
    }

    /// <summary>
    /// Retrieve a list of roles that a user belongs to.
    /// </summary>
    /// <param name="userId">The id of the user to retrieve roles for.</param>
    /// <returns>A list of roles that the user belongs to, or a 400 error if the user id is invalid.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserView"/> permission.
    /// </remarks>
    [HttpGet("get-roles/{userId:guid}")] [HasAuthority(Permissions.UserView)]
    public async Task<IActionResult> GetRolesFromUser(Guid userId)
    {
        try
        {
            var res = await userAppService.FindRolesByUser(userId);
            return Ok(res);
        }
        catch (Exception)
        {
            return BadRequest(new { message = "User Id not found" });
        }
    }

    /// <summary>
    /// Locks or unlocks a user based on their current status.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to lock or unlock.</param>
    /// <returns>A response indicating the success or failure of the operation.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserUpdate"/> and <see cref="Permissions.Admin"/> permissions.
    /// </remarks>
    [HttpPut("lock/{userId:guid}")]
    [HasAllAuthorities(Permissions.UserUpdate, Permissions.Admin)]
    public async Task<IActionResult> LockOrUnlock(Guid userId)
    {
        try
        {
            var result = await userAppService.LockOrUnlockUser(userId);
            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Resets the user's password.
    /// </summary>
    /// <param name="req">Request object containing user ID and new password.</param>
    /// <returns>A result indicating the success or failure of the password reset operation.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserUpdate"/> and <see cref="Permissions.Admin"/> permissions.
    /// </remarks>
    [HttpPut("reset-password")]
    [HasAllAuthorities(Permissions.UserUpdate, Permissions.Admin)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        try
        {
            var result = await userAppService.ResetPassword(req.UserId, req.Password);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    /// <summary>
    /// Updates the roles that a user belongs to.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="roleIds">A list of role IDs to add the user to.</param>
    /// <returns>An IActionResult indicating the result of the update operation.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserUpdate"/> permission.
    /// </remarks>
    [HttpPut("role-update/{userId:guid}")]
    [HasAllAuthorities(Permissions.UserUpdate, Permissions.Admin)]
    public async Task<IActionResult> UpdateRoles(Guid userId, List<int> roleIds)
    {
        var result = await userAppService.ChangeUserRoles(userId, roleIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Updates the organizations that asign to an user.
    /// </summary>
    /// <param name="userId">The ID of the user to update.</param>
    /// <param name="orgIds">A list of organization IDs to add the user to.</param>
    /// <returns>An IActionResult indicating the result of the update operation.</returns>
    /// <remarks>
    /// Requires the <see cref="Permissions.UserUpdate"/> permission.
    /// </remarks>
    [HttpPut("org-update/{userId:guid}")] [HasAuthority(Permissions.UserUpdate)]
    public async Task<IActionResult> UpadteOrganizations(Guid userId, List<Guid> orgIds)
    {
        var result = await userAppService.AddOrganizationToUser(userId, orgIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Checks if a username exists in the system.
    /// </summary>
    /// <param name="username">The username to check for existence.</param>
    /// <returns>returns TRUE if the username has already exist, otherwise, returns FALSE</returns>
    /// <remarks>
    /// Requires valid permissions for user data access.
    /// </remarks>
    [HttpGet("exist-username")]
    [HasAllAuthorities(Permissions.UserCreate, Permissions.UserView)]
    public async Task<IActionResult> ExistUsername([FromQuery] string username)
    {
        var result = await userAppService.ExistUsername(username);
        return Ok(result);
    }
}