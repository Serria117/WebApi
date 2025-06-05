namespace WebApp.Services.PayrollService.Dto;

public class EmployeeDisplayDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Pid { get; set; }
    public string? TaxId { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; }
    public Guid OrganizationId { get; set; }
    
    public decimal? Salary { get; set; }
    public decimal? InsuranceSalary { get; set; }
    
    public List<DependentDisplayDto> Dependents { get; set; } = [];
}