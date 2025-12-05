using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting.TaxDeclarations;

[Table("TaxReportDuties")] [Index(nameof(Name))]
public class TaxDuty : BaseEntity<int>
{
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 999999)]
    public int Order { get; set; }

    public int TaxDutyCategoryId { get; set; }

    [ForeignKey(nameof(TaxDutyCategoryId))]
    public TaxDutyCategory Category { get; set; } = null!;

}