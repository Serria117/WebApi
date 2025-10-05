using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table("ACC_CashFlowStatementItem")]
public class CashFlowStatementItem : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(5)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(5)]
    public string? ParentCode { get; set; }

    public int RegulationId { get; set; }

    public bool HasChild { get; set; } = false;

    public bool NegativeValue { get; set; } = false;
}