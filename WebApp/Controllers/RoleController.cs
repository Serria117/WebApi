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
    [HttpPost("create")]
    [HasAuthority(permission: Permissions.RoleCreate)]
    public async Task<IActionResult> CreateRole(RoleInputDto dto)
    {
        return Ok(await roleService.CreateRole(dto));
    }

    [HttpGet("all")]
    [HasAuthority(permission: Permissions.RoleView)]
    public async Task<IActionResult> GetAllRoles([FromQuery] RequestParam req)
    {
        var paging = PageRequest.GetPagingAndSortingParam(req);
        var result = await roleService.GetAllRoles(paging);
        return Ok(result);
    }

    /// <summary>
    /// Update a role with permissions and users
    /// </summary>
    /// <param name="id">The role's ID</param>
    /// <param name="dto">The new data of the role</param>
    /// <returns></returns>
    [HttpPut("edit/{id:int}")]
    public async Task<IActionResult> UpdateRole(int id, RoleUpdatetDto dto)
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

    /// <summary>
    /// Find a role by its ID
    /// </summary>
    /// <param name="id">Role's ID</param>
    /// <returns>The role if found, else 404 not found</returns>
    [HttpGet("find/{id:int}")]
    public async Task<IActionResult> FindRoleById(int id)
    {
        var result = await roleService.FindRoleById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
