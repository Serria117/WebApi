using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.UserService;
using WebApp.Services.UserService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/role")][Authorize]
public class RoleController(IRoleAppService roleService, IPermissionAppService permissionService) : ControllerBase
{
    /// <summary>
    /// Creates a new role based on the provided role input data.
    /// </summary>
    /// <param name="dto">The data transfer object containing the details for creating a new role, including its name, description, permissions, and associated users.</param>
    /// <returns>Returns an IActionResult containing the created role's details.</returns>
    [HttpPost("create")]
    [HasAuthority(permission: Permissions.RoleCreate)]
    public async Task<IActionResult> CreateRole(RoleInputDto dto)
    {
        return Ok(await roleService.CreateRole(dto));
    }

    /// <summary>
    /// Retrieves a paginated and sorted list of all roles based on the provided request parameters.
    /// </summary>
    /// <param name="req">The request parameters for pagination, sorting, and filtering.</param>
    /// <returns>Returns an IActionResult containing the paginated list of roles.</returns>
    [HttpGet]
    [HasAuthority(permission: Permissions.RoleView)]
    public async Task<IActionResult> GetAllRoles([FromQuery] RequestParam req)
    {
        var paging = PageRequest.BuildRequest(req);
        var result = await roleService.GetAllRoles(paging);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a role by its specific ID.
    /// </summary>
    /// <param name="id">The unique identifier of the role to retrieve.</param>
    /// <returns>Returns an IActionResult containing the role details if found, or a not found result if the role does not exist.</returns>
    [HttpGet("{id:int}")] [HasAuthority(permission: Permissions.RoleView)]
    public async Task<IActionResult> GetRoleById(int id)
    {
        var result = await roleService.GetRoleById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
    
    /// <summary>
    /// Update a role with permissions and users
    /// </summary>
    /// <param name="id">The role's ID</param>
    /// <param name="dto">The new data of the role</param>
    /// <returns></returns>
    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, RoleInputDto dto)
    {
        await roleService.UpdateRole(id, dto);
        return Ok();
    }

    [HttpGet("permissions-in-role/{roleId:int}")]
    [HasAuthority(permission: Permissions.RoleView)]
    public async Task<IActionResult> GetPermissionsInRole(int roleId)
    {
        var result = await roleService.GetAllPermissionsInRole(roleId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("all-permissions")][HasAuthority(permission: Permissions.RoleView)]
    public async Task<IActionResult> GetAllPermissionsInSystem()
    {
        var result = await permissionService.GetAllPermissionsInSystem();
        return Ok(result);
    }
}
