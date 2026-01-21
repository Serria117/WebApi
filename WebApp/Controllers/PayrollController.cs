using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/payroll"), Authorize]
public partial class PayrollController(IPayrollAppService service) : ControllerBase
{
    [HttpPost("period/create")]
    public async Task<IActionResult> CreatePayrollPeriods(PayrollPeriodCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        dto.Valid();
        var result = await service.CreatePayrollPeriodsAsync(dto.Year, dto.Weekend);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("period/{id:long}")]
    public async Task<IActionResult> GetPayrollPeriodById(long id)
    {
        var result = await service.GetPayrollPeriodAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("period/all")]
    public async Task<IActionResult> GetPayrollPeriods([FromQuery] PayrollPeriodQuery query)
    {
        var result = await service.GetYearPayrollPeriod(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

}