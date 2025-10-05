using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.CorporateTax;

[Table("ACC_CorporateTaxIncomeStatementEntry")]
public class CorporateTaxIncomeStatementEntry : BaseEntity<long>
{
    [ForeignKey(nameof(IncomeStatementItem))]
    public int IncomeStatementItemId { get; set; }

    public CoporateTaxIncomeStatementItem IncomeStatementItem { get; set; } = null!;

    public decimal Value { get; set; } = 0;
}