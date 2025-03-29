using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities;

[Table("RegionDistrict")]
public class District: BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string AlterName { get; set; } = string.Empty;

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public Province? Province { get; set; }
}