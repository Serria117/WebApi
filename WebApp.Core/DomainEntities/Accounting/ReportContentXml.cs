using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Core.DomainEntities.Accounting.FinancialStatement;

namespace WebApp.Core.DomainEntities.Accounting;

[Table("ACC_XmlReport")]
public class ReportContentXml : BaseEntity<int>
{
    [MaxLength(255)]
    public string? FileName { get; set; } = string.Empty;

    [Column(TypeName = "NVARCHAR(MAX)")]
    public string Content { get; set; } = string.Empty;

    [Column(TypeName = "CHAR(26)")]
    public string FinancialReportId { get; set; } = string.Empty;

    [ForeignKey(nameof(FinancialReportId))]
    public FinancialReportWork FinancialReportWork { get; set; } = null!;
}