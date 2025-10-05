using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table(name: "ACC_TrialBalanceEntries")]
[Index(nameof(AccountCode))]
public class TrialBalanceEntry : BaseEntity<long>
{
    [MaxLength(10)]
    public string AccountCode { get; set; } = string.Empty;

    [MaxLength(10)]
    public string? ParentCode { get; set; }


    [MaxLength(255)]
    public string? Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpenDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpenCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AriseDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AriseCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CloseDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CloseCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? InvalidNetBalanceDifference { get; set; }

    public bool IsUserInput { get; set; } = false;

    public bool IsMatchRegulation { get; set; } = true;

    public bool MappedSuccess { get; set; } = false;

    public bool? NetBalanceValid { get; set; } = null; // null: not checked, true: valid, false: invalid

    [Column(TypeName = "CHAR(26)")]
    public string? FinancialReportWorkId { get; set; }

    [ForeignKey(nameof(FinancialReportWorkId))]
    public FinancialReportWork? FinancialReportWork { get; set; }

    public int? UserInputTrialBalanceId { get; set; }

    [ForeignKey("UserInputTrialBalanceId")]
    public UserInputTrialBalance? InputTrialBalance { get; set; }
}