using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_TaxRate")]
public class TaxRate : BaseEntity<long>
{
    [Column(TypeName = "decimal(5,4)")]
    public decimal Rate { get; set; }

    public TaxRateType TaxRateType { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MinIncome { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxIncome { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DeductibleAmount { get; set; } = 0;

    public long TaxRateGroupId { get; set; }

    [ForeignKey(nameof(TaxRateGroupId))]
    public TaxRateTable TaxRateTable { get; set; } = null!;
}

public enum TaxRateType
{
    Progressive,
    Full,
    NonResident
}