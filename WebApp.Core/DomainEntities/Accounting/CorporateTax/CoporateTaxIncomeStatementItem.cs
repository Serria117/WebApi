using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.CorporateTax;

[Table("ACC_CoporateTaxIncomeStatementItem")]
public class CoporateTaxIncomeStatementItem : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public bool HasChild { get; set; } = false;
    public bool NegativeValue { get; set; } = false;
}