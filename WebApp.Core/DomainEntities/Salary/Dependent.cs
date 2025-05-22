using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class Dependent: BaseEntity<int>
{
    public string FullName { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Pid { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid EmployeeId { get; set; } // Foreign key

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
}