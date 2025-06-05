using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class Employee : BaseEntityAuditable<Guid>
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? Pid { get; set; } // Personal ID

    [MaxLength(20)]
    public string? TaxId { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public bool IsActive { get; set; } = true;

    public required Organization Organization { get; set; }

    [ForeignKey(nameof(Organization))]
    public Guid OrganizationId { get; set; } // Foreign key

    [Column(TypeName = "decimal(18,2)")]
    public decimal? BaseSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? InsuranceSalary { get; set; }

    [ForeignKey(nameof(CostDepartment))]
    public int? CostDepartmentId { get; set; } // Foreign key

    public CostDepartment? CostDepartment { get; set; } // Navigation property

    public List<BaseSalary> BaseSalaries { get; set; } = []; // Navigation property
    
    public List<InsuranceSalary> InsuranceSalaries { get; set; } = []; // Navigation property
    public List<Dependent> Dependents { get; set; } = []; // Navigation property
}