using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.PayrollService.Dto;

public class EmployeeCreateDto
{
    [MaxLength(255)]
    public required string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Pid { get; set; } // Personal ID

    [MaxLength(20)]
    public string? TaxId { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }

    public decimal Salary { get; set; }
    public decimal InsuranceSalary { get; set; }
    public List<DependentCreateDto> Dependents { get; set; } = [];

   // public Guid OrganizationId { get; set; }
}