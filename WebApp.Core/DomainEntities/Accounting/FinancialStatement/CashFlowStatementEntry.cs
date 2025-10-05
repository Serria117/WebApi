using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table("ACC_CashFlowStatementEntry")]
public class CashFlowStatementEntry : BaseEntityAuditable<long>
{
    public int CashFlowStatementItemId { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(5)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(5)]
    public string? ParentCode { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ThisYear { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LastYear { get; set; }

    [Column(TypeName = "CHAR(26)")]
    public required string FinancialReportWorkId { get; set; }

    [ForeignKey("FinancialReportWorkId")]
    public FinancialReportWork FinancialReportWork { get; set; } = null!;
}