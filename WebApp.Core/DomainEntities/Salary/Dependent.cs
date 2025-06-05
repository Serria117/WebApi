using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class Dependent: BaseEntity<int>
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;
    [MaxLength(20)]
    public string? TaxId { get; set; }
    [MaxLength(20)]
    public string? Pid { get; set; }
    [MaxLength(30)]
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid EmployeeId { get; set; } // Foreign key

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
}