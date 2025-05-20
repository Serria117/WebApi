using WebApp.Services.PayrollService.Dto;

namespace WebApp.Payloads;

/// <summary>
/// Represents a request to create multiple employees within an organization.
/// </summary>
/// <remarks>
/// Organization ID is required to associate the employees with a specific organization.
/// Each employee's details are provided in the Emp property, which is a list of EmployeeCreateDto.
/// </remarks>
public class CreateManyEmployeeRequest
{
    public List<EmployeeCreateDto> Employees { get; set; } = [];
}