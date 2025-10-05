using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

/// <summary>
/// Collect the user input trial balance entries for a specific year and organization.
/// </summary>
[Table(name: "ACC_UserInputBalancesheet")]
public class UserInputTrialBalance : BaseEntityAuditable<int>
{
    public int Year { get; set; }
    public Guid OrganizationId { get; set; }
    public int Regulation { get; set; }
    public ICollection<TrialBalanceEntry> Entries { get; set; } = [];

    [ForeignKey(nameof(FinancialReportWork))]
    [Column(TypeName = "CHAR(26)")]
    public string? FinancialReportWorkId { get; set; }
    public FinancialReportWork? FinancialReportWork { get; set; }

    
}