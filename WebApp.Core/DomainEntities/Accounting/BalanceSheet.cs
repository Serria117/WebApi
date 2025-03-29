namespace WebApp.Core.DomainEntities.Accounting;

public class BalanceSheet : BaseEntityAuditable<int>
{
    public int Year { get; set; }
    public int Version { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public Organization? Organization { get; set; }
    public HashSet<BalanceSheetDetail> Details { get; set; } = [];
    
    public List<BalanceSheetEntry> Entries { get; set; } = [];
    public ImportedBalanceSheet? ImportedBalanceSheet { get; set; }
}