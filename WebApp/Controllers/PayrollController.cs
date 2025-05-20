using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;
[ApiController][Authorize][Route("api/payroll")]
public class PayrollController(IPayrollAppService payrollService) : ControllerBase
{
    [HttpPost("/eployee/create")]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto input)
    {
        try
        {
            var response = await payrollService.CreateEmployeeAsync(input);
            return Ok(response);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}