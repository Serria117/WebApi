using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;
[Table(name: "ACC_TrialBalances")]
public class TrialBalance : BaseEntityAuditable<int>
{
    public int Year { get; set; }
    public Guid OrganizationId { get; set; }
    public int Regulation { get; set; }

    public ICollection<TrialBalanceEntry> Entries { get; set; } = [];

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;
    
    [ForeignKey(nameof(FinancialReportWork))]
    public int? FinancialReportWorkId { get; set; }
    public FinancialReportWork? FinancialReportWork { get; set; }
}
