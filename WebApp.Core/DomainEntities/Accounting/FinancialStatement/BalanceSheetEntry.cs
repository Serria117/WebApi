using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table(name: "ACC_BalanceSheetEntries")]
public class BalanceSheetEntry : BaseEntity<int>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal EndingBalance { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BeginingBalance { get; set; }
    [MaxLength(255)]
    public string? Name { get; set; } = string.Empty;
    [MaxLength(5)] 
    public string? Code { get; set; } = string.Empty;

    public int BalanceSheetItemId { get; set; }

    [Column(TypeName = "CHAR(26)")]
    public string FinancialReportId { get; set; } = string.Empty;
    [MaxLength(5)]
    public string? ParentCode { get; set; }
    public bool HasChildren { get; set; } = false;

    [ForeignKey(nameof(FinancialReportId))]
    public FinancialReportWork FinancialReportWork { get; set; } = null!;

    [ForeignKey(nameof(BalanceSheetItemId))]
    public BalanceSheetItem BalanceSheetItem { get; set; } = null!;
}