using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

[ApiController]
[Route("/api/employee")]
[Authorize]
public class EmployeeController(IPayrollAppService service) : ControllerBase
{
    [HttpPost(Api.Create)]
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

    [HttpGet(Api.GetAll)]
    public async Task<IActionResult> GetAllEmployee([FromQuery] EmployeeQuery query)
    {
        var result = await service.GetEmployeesAsync(query);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet(Api.GetAll + "/{periodId:long}")]
    public async Task<IActionResult> GetEmployeeByPeriod([FromRoute] long periodId)
    {
        var emp = await service.GetEmployeesInPeriod(periodId);
        return emp.Success ? Ok(emp) : BadRequest(emp);
    }

    [HttpPut($"{Api.Update}/{{id:long}}")]
    public async Task<IActionResult> UpdateEmployee(long id, EmployeeUpdate dto)
    {
        var result = await service.UpdateEmployeeAsync(id, dto);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [HttpDelete(Api.Delete)]
    public async Task<IActionResult> DeleteEmployees([FromBody] EmployeeDelete request)
    {
        if (request.IdList.Count == 0) return BadRequest("Nothing to delete.");
        var result = await service.DeleteEmployeesAsync(request.IdList);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("dependent/change")]
    public async Task<IActionResult> ChangeDependentsForEmployee(DependentsToAdd request)
    {
        var result = await service.EditDependentsInEmployeeAsync(request.EmpId, request.Dependents);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost($"department/{Api.Create}")]
    public async Task<IActionResult> CreateDepartment(DepartmentCreate dto)
    {
        await service.CreateDepartmentAsync(dto);
        return Ok();
    }

    [HttpGet($"department/{Api.GetAll}")]
    public async Task<IActionResult> GetDepartments()
    {
        var result = await service.GetDepartmentsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet($"department/{Api.GetById}/{{id}}")]
    public async Task<IActionResult> GetDepartmentById(string id)
    {
        var result = await service.GetDepartmentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}