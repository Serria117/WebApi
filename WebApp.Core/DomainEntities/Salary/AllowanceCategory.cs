using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

[Table("PR_AllowanceCategory")]
public class AllowanceCategory : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; } // Optional description for the category

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; } // Nullable for future updates

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Limit { get; set; } // Optional limit for the allowance category, can be null if no limit applies

    // Optional unit for the limit (e.g., "month", "year"), can be null if no limit applies
    [MaxLength(50)]
    public string? LimitPerUnit { get; set; }
}