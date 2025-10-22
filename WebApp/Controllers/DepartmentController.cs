using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("api/payroll/departments")] [Authorize]
public class DepartmentController(IPayrollAppService payrollService) : ControllerBase
{
    [HttpGet($"{ApiTemplate.GetAll}")]
    public async Task<IActionResult> GetDepartments([FromQuery]RequestParam param)
    {
        param.Valid();
        var departments = await payrollService.GetDepartmentsAsync(param);
        return Ok(departments);
    }

    [HttpPost("change-employee")]
    public async Task<IActionResult> ChangeDependentsForEmployee(DependentsToAdd request)
    {
        var result = await payrollService.EditDependentsInEmployeeAsync(request.EmpId, request.Dependents);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost($"{ApiTemplate.Create}")]
    public async Task<IActionResult> CreateDepartment(DepartmentCreate dto)
    {
        await payrollService.CreateDepartmentAsync(dto);
        return Ok();
    }


    [HttpGet($"{ApiTemplate.GetById}/{{id}}")]
    public async Task<IActionResult> GetDepartmentById(string id)
    {
        var result = await payrollService.GetDepartmentByIdAsync(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
    
    [HttpGet("exist/{name}")]
    public async Task<IActionResult> CheckDepartmentExists(string name)
    {
        name = name.RemoveSpace() ?? string.Empty;
        var result = await payrollService.IsDepartmentExistAsync(name);
        return Ok(result);
    }
}