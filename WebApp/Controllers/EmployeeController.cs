using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

[ApiController]
[Route("/api/payroll/employees")]
[Authorize]
public class EmployeeController(IPayrollAppService service) : ControllerBase
{
    [HttpPost(ApiTemplate.Create)]
    public async Task<IActionResult> CreateEmployee(EmployeeCreateDto dto)
    {
        var result = await service.CreateEmployeeAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetEmployeeById(long id)
    {
        var result = await service.GetEmployeeById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpGet(ApiTemplate.GetAll)]
    public async Task<IActionResult> GetAllEmployee([FromQuery] EmployeeQuery query)
    {
        var result = await service.GetEmployeesAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet(ApiTemplate.GetAll + "/{periodId:long}")]
    public async Task<IActionResult> GetEmployeeByPeriod([FromRoute] long periodId)
    {
        var emp = await service.GetEmployeesInPeriod(periodId);
        return emp.Success ? Ok(emp) : BadRequest(emp);
    }

    [HttpPut($"{ApiTemplate.Update}/{{id:long}}")]
    public async Task<IActionResult> UpdateEmployee([FromRoute]long id, EmployeeUpdate dto)
    {
        var result = await service.UpdateEmployeeAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete(ApiTemplate.Delete)]
    public async Task<IActionResult> DeleteEmployees([FromBody] EmployeeDelete request)
    {
        if (request.IdList.Count == 0) return BadRequest("Nothing to delete.");
        var result = await service.DeleteEmployeesAsync(request.IdList);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    
}