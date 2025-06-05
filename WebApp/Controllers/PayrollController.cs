using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Payloads.Payroll;
using WebApp.Services.PayrollService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

[ApiController] [Authorize] [Route("api/payroll")]
public class PayrollController(IPayrollAppService payrollService) : ControllerBase
{
    #region Employee
    
    /// <summary>
    /// Creates a new employee in the payroll system.
    /// </summary>
    /// <param name="input">The details of the employee to be created.</param>
    /// <returns>Returns an <see cref="IActionResult"/> indicating the result of the creation operation.</returns>
    [HttpPost("employee/create")] [HasAuthority(Permissions.EmployeeCreate)]
    public async Task<IActionResult> CreateEmployee([FromBody] EmployeeCreateDto input)
    {
        var result = await payrollService.CreateEmployeeAsync(input);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
    
    /// <summary>
    /// Creates multiple employees in the payroll system.
    /// </summary>
    /// <param name="request">The request containing a collection of employee data to be created.</param>
    /// <returns>Returns an <see cref="IActionResult"/> indicating the result of the bulk creation operation.</returns>
    [HttpPost("employee/create-many")] [HasAuthority(Permissions.EmployeeCreate)]
    public async Task<IActionResult> CreateManyEmployee([FromBody] CreateManyEmployeeRequest request)
    {
        var result = await payrollService.CreateManyEmployeesAsync(request.Employees);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    /// <summary>
    /// Retrieves a paginated list of employees based on the provided query parameters.
    /// </summary>
    /// <param name="parameters">The parameters used for filtering, sorting, and pagination of employees.</param>
    /// <returns>Returns an <see cref="IActionResult"/> containing the list of employees if successful; otherwise, a bad request response.</returns>
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

    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee.</param>
    /// <returns>Returns an <see cref="IActionResult"/> containing the employee details if found; otherwise, a bad request response.</returns>
    [HttpGet("employee/{id:guid}")] [HasAuthority(Permissions.EmployeeView)]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var result = await payrollService.GetEmployeeByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result.Message);
    }
    
    #endregion

    #region Payroll Period

    
    /// <summary>
    /// Initializes a payroll period for a specific year.
    /// </summary>
    /// <param name="request">The request containing the details required to initialize the payroll period.</param>
    /// <returns>Returns an <see cref="IActionResult"/> indicating the result of the initialization operation.</returns>
    [HttpPost("payroll-period/init")] [HasAuthority(Permissions.PayrollCreate)]
    public async Task<IActionResult> InitPayrollPeriodAsync(InitPayrollPeriodRequest request)
    {
        var result = await payrollService.InitPayrollByYearAsync(request);
        return result.Success 
            ? Ok(result) 
            : StatusCode(result.ToHttpStatusCode(), result.Message);
    }
    
    
    /// <summary>
    /// Creates a new payroll period.
    /// </summary>
    /// <param name="input">The details required to create a payroll period.</param>
    /// <returns>Returns an <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPost("payroll-period/create")] [HasAuthority(Permissions.PayrollCreate)]
    public async Task<IActionResult> CreatePayrollPeriod([FromBody] PayrollPeriodCreateDto input)
    {
        var result = await payrollService.CreatePayrollPeriodAsync(input);
        return result.Success 
            ? Ok(result) 
            : StatusCode(result.ToHttpStatusCode(), result.Message);
    }
    
    /// <summary>
    /// Retrieves a list of payroll periods based on the specified request parameters.
    /// </summary>
    /// <param name="request">The request parameters used to filter and paginate payroll periods.</param>
    /// <returns>Returns an <see cref="IActionResult"/> containing the list of payroll periods if successful; otherwise, a bad request or error response.</returns>
    [HttpGet("payroll-period/all")] [HasAuthority(Permissions.PayrollView)]
    public async Task<IActionResult> GetPayrollPeriods([FromQuery] PayrollPeriodRequest request)
    {
        try
        {
            var result = await payrollService.GetPayrollPeriods(request);
            return result.Success 
                ? Ok(result) 
                : StatusCode(result.ToHttpStatusCode(), result.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

   /// <summary>
   /// Retrieves a payroll period by its unique integer identifier.
   /// </summary>
   /// <param name="id">The unique identifier of the payroll period.</param>
   /// <returns>Returns an <see cref="IActionResult"/> containing the payroll period details if found; otherwise, a bad request or error response.</returns>
    [HttpGet("payroll-period/{id:int}")] [HasAuthority(Permissions.PayrollView)]
    public async Task<IActionResult> GetPayrollPeriodById(int id)
    {
        try
        {
            var result = await payrollService.GetPayrollPeriodById(id);
            return result.Success 
                ? Ok(result) 
                : StatusCode(result.ToHttpStatusCode(), result.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    #endregion
    
}