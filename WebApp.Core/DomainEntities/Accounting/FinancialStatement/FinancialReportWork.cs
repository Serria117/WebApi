using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

/// <summary>
/// Represents a full report of the financial statement.<br/>
/// Each report is based on a specific regulation and year,
/// and contains detailed entries for trial balances, balance sheets, and income statements. <br/>
/// Each report belong to an organization only.
/// </summary>
[Table(name: "ACC_FinancialReportWorks")]
public class FinancialReportWork : BaseEntityAuditable<string>
{
    [Key]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();
    public int Regulation { get; set; }
    public int Year { get; set; }
    public Guid? OrganizationId { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReportDate { get; set; }

    [MaxLength(5)]
    public string FirstFiscalDate { get; set; } = string.Empty;

    [MaxLength(255)]
    public string TaxAgencyName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string TaxAgencyCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Province { get; set; }

    [MaxLength(10)]
    public string? ProvinceCode { get; set; }

    [MaxLength(255)]
    public string? District { get; set; }

    [MaxLength(10)]
    public string? DistrictCode { get; set; }

    [MaxLength(20)]                                                                                                 
    public string? Status { get; set; } = ReportStatus.New;                                                                         

    [Column(TypeName = "CHAR(26)")]
    public string? LastYearReportId { get; set; }

    public Organization? Organization { get; set; }
    public UserInputTrialBalance? UserInput { get; set; }
    public ICollection<TrialBalanceEntry> TrialBalanceEntries { get; set; } = [];
    public ICollection<BalanceSheetEntry> BalanceSheetEntries { get; set; } = [];
    public ICollection<IncomeStatementEntry> IncomeStatementEntries { get; set; } = [];
    
    public ReportContentXml? Xml { get; set; }
}

public struct ReportStatus
{
    public const string New = "New";
    public const string Pending = "Pending";
    public const string Finish = "Finished";
}