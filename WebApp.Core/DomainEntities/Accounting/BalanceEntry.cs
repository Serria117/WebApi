using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Core.DomainEntities.Accounting;

[Table(name: "ACC_BalanceEntry")]
public class BalanceEntry : BaseEntity<long>
{
    [MaxLength(10)]
    public string AccountCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Name { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpenDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpenCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AriseDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AriseCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CloseDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CloseCredit { get; set; }

    public bool IsUserInput { get; set; } = false;
}