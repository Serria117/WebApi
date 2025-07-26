using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;
[ApiController][Route("/api/payroll/allowance")][Authorize]
public class AllowanceController(IPayrollAppService service) : ControllerBase
{
    [HttpGet(Api.GetAll)]
    public async Task<IActionResult> GetAllowanceTypes()
    {
        var result = await service.GetAllowanceTypes();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Create new allowance type.
    /// </summary>
    /// <param name="input">Allowance detail informations</param>
    /// <returns></returns>
    [HttpPost(Api.Create)]
    public async Task<IActionResult> CreateAllowanceType(AllowanceTypeCreate input)
    {
        var result = await service.CreateAllowanceType(input);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}