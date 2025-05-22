using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

[ApiController] [Authorize] [Route("api/payroll")]
public class PayrollController(IPayrollAppService payrollService) : ControllerBase
{
    [HttpPost("employee/create")] [HasAuthority(Permissions.EmployeeCreate)]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto input)
    {
        var response = await payrollService.CreateEmployeeAsync(input);
        return response.Success ? Ok(response) : BadRequest(response.Message);
    }

    [HttpPost("employee/create-many")] [HasAuthority(Permissions.EmployeeCreate)]
    public async Task<IActionResult> CreateManyEmployee([FromBody] CreateManyEmployeeRequest request)
    {
        var response = await payrollService.CreateManyEmployeesAsync(request.Employees);
        return response.Success ? Ok(response) : BadRequest(response.Message);
    }

    [HttpGet("employee/all")] [HasAuthority(Permissions.EmployeeView)]
    public async Task<IActionResult> GetEmployees([FromQuery] RequestParam parameters)
    {
        try
        {
            var request = PageRequest.BuildRequest(parameters);
            var result = await payrollService.GetEmployeesAsync(request);
            return result.Success ? Ok(result) : BadRequest(result.Message);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("employee/{id:guid}")] [HasAuthority(Permissions.EmployeeView)]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var result = await payrollService.GetEmployeeByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
}