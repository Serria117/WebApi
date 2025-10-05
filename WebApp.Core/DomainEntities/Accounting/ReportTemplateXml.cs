using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting;

[Table("ACC_ReportTemplateXml")]
public class ReportTemplateXml : BaseEntity<int>
{
    public int RegulationId { get; set; }

    [ForeignKey(nameof(RegulationId))]
    public AccountingRegulation Regulation { get; set; } = null!;

    [Column(TypeName = "NVARCHAR(MAX)")]
    public string XmlTemplate { get; set; } = string.Empty;
}