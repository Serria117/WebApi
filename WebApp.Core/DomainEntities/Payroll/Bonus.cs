using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Payroll;

/// <summary>
/// Stores information about bonuses given to employees.
/// </summary>
[Table("PR_Bonus")]
[Index(nameof(EffectiveDate)), Index(nameof(EndDate))]
public class Bonus : BaseEntity<long>
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; } = 0;

    public long EmployeeId { get; set; }
    public long BonusTypeId { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(BonusTypeId))]
    public BonusType BonusType { get; set; } = null!;
}