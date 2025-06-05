using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class IncomeTaxBracket : BaseEntityAuditable<int>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal Min { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Max { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CumulativeTax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxRate { get; set; }

    public int Order { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; } // nullable for future updates

    [ForeignKey(nameof(TaxBracketGroupId))]
    public TaxBracketGroup? TaxBracketGroup { get; set; } // Navigation property

    public int? TaxBracketGroupId { get; set; } // Foreign key
}