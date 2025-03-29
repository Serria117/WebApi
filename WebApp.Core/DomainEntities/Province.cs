using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

[Table("RegionProvince")]
public class Province : BaseEntity<int>
{
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? AlterName { get; set; }

    public HashSet<District> Districts { get; set; } = [];

    public HashSet<TaxOffice> TaxOffices { get; set; } = [];
}