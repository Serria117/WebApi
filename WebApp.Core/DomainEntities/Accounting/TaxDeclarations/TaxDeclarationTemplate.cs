using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.TaxDeclarations;
/// <summary>
/// Store tax declaration template for validation
/// </summary>
[Table("TaxDeclarationTemplates")]
public class TaxDeclarationTemplate : BaseEntity<string>
{
    [MaxLength(26)]
    public new string Id { get; set; } = Ulid.NewUlid().ToString();
    
    //Store the schema of the template in xml format
    [Column(TypeName = "NVARCHAR(MAX)")]
    public string? Schema { get; set; }

    [MaxLength(20)]                                                                                             
    public string? Code { get; set; }
    
    [MaxLength(255)]                                                                                                    
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]                                                                                             
    public string? Description { get; set; }

    public int? TaxDutyId { get; set; }

    [ForeignKey(nameof(TaxDutyId))]                                                                              
    public TaxDuty? TaxDuty { get; set; }
}