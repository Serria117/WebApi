using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.PayrollService.Dto;

public class EmployeeCreateDto
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string? Code { get; set; }
    public string? TaxId { get; set; }
    public string? PersonalId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal SalaryValue { get; set; } = 0;
    public decimal? InsuranceSalaryValue { get; set; }
    public DateTime SalaryStartDate { get; set; }
    public DateTime SalaryEndDate { get; set; }

    public string? OtherDocumentId { get; set; }
    public string? OtherDocument { get; set; }
    public ICollection<DependentCreateDto> Dependents { get; set; } = [];
    public ICollection<AllowanceCreate> Allowances { get; set; } = [];
}

public class EmployeeUpdate : EmployeeCreateDto
{
    public long? Id { get; set; }
    public new ICollection<DependentUpdate> Dependents { get; set; } = [];
}

public class EmployeeDisplay
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? TaxId { get; set; }
    public string? PersonalId { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? OtherDocumentId { get; set; }
    public string? OtherDocument { get; set; }
    public ICollection<DependentDisplay> Dependents { get; set; } = [];
    public ICollection<SalaryDisplay> Salaries { get; set; } = [];
}

public class EmployeeDelete
{
    public List<long> IdList { get; set; } = [];
}

public class EmployeeQuery
{
    public string? Keyword { get; set; }
    public int? Year { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class DependentCreateDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long EmployeeId { get; set; }
    public string? PersonalId { get; set; }
    public string? TaxId { get; set; }
    public string? Relationship { get; set; }
    public string? OtherDocumentId { get; set; }
    public string? OtherDocument { get; set; }
}

public class DependentUpdate : DependentCreateDto
{
    public long? Id { get; set; }
}

public class DependentDisplay : DependentCreateDto
{
    public long Id { get; set; }
}

public class DependentsToAdd
{
    public long EmpId { get; set; }
    public ICollection<DependentCreateDto> Dependents { get; set; } = [];
}

public class AllowanceCreate
{
    public decimal Amount { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long AllowanceTypeId { get; set; }
    public long EmployeeId { get; set; }
}