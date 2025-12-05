using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting.TaxDeclarations;

[Index(nameof(Code), IsUnique = true)]
[Index(nameof(UnsignName))]
public class TaxProcedure : BaseEntity<int>
{
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(550)]
    public string UnsignName { get; set; } = string.Empty;

    [Column(TypeName = "NVARCHAR(MAX)")]
    public string? Description { get; set; }

    public int Order { get; set; }
}
