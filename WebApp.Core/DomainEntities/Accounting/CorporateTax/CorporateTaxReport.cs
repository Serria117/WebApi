using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.CorporateTax;

[Table("ACC_CorporateTaxReport")]
public class CorporateTaxReport : BaseEntityAuditable<string>
{
    [Key]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();

    public int Year { get; set; }
    public Guid OrganizationId { get; set; }
    public string? FinancialReportId { get; set; }

    [ForeignKey(nameof(OrganizationId))]
    public Organization Organization { get; set; } = null!;

    public ICollection<CorporateTaxMainEntry> MainEntries { get; set; } = [];
    public ICollection<CorporateTaxIncomeStatementEntry> IncomeStatementEntries { get; set; } = [];
}