using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table(name: "ACC_IncomeStatementEntries")]
public class IncomeStatementEntry : BaseEntity<int>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal LastYear { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ThisYear { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(5)]
    public string Code { get; set; } = string.Empty;
    public int IncomeStatementItemId { get; set; }
    
    [Column(TypeName = "CHAR(26)")]
    public string FinancialReportWorkId { get; set; } = string.Empty;

    [ForeignKey(nameof(IncomeStatementItemId))]
    public IncomeStatementItem IncomeStatementItem { get; set; } = null!;

    [ForeignKey(nameof(FinancialReportWorkId))]
    public FinancialReportWork FinancialReportWork { get; set; } = null!;
}