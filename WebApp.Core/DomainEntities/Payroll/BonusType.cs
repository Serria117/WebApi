using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_BonusType")]
public class BonusType : BaseEntity<long>
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Code { get; set; }

    public ICollection<Bonus> Bonus { get; set; } = [];
}