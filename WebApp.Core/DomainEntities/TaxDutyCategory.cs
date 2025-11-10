using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

[Table("TaxDutyCategories")]
public class TaxDutyCategory : BaseEntity<int>
{
    [Column(TypeName = "nvarchar(255)")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(50)")]
    public string Code { get; set; } = string.Empty;

    [Column(TypeName = "nvarchar(255)")]
    public string UnsignName { get; set; } = string.Empty;

    public ICollection<TaxReportDuty> TaxReportDuties { get; set; } = []; //Navigation property
}