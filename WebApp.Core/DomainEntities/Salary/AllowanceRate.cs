using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

/// <summary>
/// Thông tin về mức phụ cấp, trợ cấp của từng doanh nghiệp.
/// </summary>
public class AllowanceRate : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public int AllowanceCategoryId { get; set; }

    [ForeignKey(nameof(AllowanceCategoryId))]
    public AllowanceCategory AllowanceCategory { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public bool IsTaxable { get; set; }

    [MaxLength(255)]
    public string? JobTitle { get; set; } //Applied to all job titles if null

    public Guid? OrganizationId { get; set; } //null if applied to all organizations

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; } // nullable for future updates
}