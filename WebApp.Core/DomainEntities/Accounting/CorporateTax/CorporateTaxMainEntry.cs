using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.CorporateTax;

public class CorporateTaxMainEntry : BaseEntity<long>
{
    public int MainItemId { get; set; }

    [ForeignKey(nameof(MainItemId))]
    public CorporateTaxMainItem CorporateTaxMainItem { get; set; } = null!;

    public decimal Value { get; set; } = 0;
}