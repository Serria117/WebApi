using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.UserService.AdminService;

namespace WebApp.Controllers;

[ApiController] [Route("/api/admin")] [Authorize]
public class AdminController(IAdminAppService adminService) : ControllerBase
{
    [HttpGet("menu")]
    public async Task<IActionResult> GetAllMenuItem()
    {
        var result = await adminService.GetAllMenus();
        return result.Success ? Ok(result.Data) : BadRequest(result.Message);
    }
}