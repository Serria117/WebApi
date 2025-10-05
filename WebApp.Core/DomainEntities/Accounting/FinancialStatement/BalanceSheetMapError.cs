using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table("ACC_BalanceSheetError")]
public class BalanceSheetMapError : BaseEntity<long>
{
    public int FinancialReportId { get; set; }

    [ForeignKey("FinancialReportId")]
    public FinancialReportWork FinancialReportWork { get; set; } = null!;

    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorAccountCode { get; set; } = string.Empty;
}