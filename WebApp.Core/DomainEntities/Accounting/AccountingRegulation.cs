using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities.Accounting;
[Table(name: "ACC_Regulation")]
public class AccountingRegulation : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(500)]
    public string? Description { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}
