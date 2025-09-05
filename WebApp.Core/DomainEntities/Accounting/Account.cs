using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Principal;
using System.Text;
using WebApp.Enums.Accounting;

namespace WebApp.Core.DomainEntities.Accounting;
[Table(name: "ACC_Account")]
public class Account : BaseEntity<int>
{
    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;
    [Range(0, 10)]
    public int Level { get; set; }
    public string? ParentCode { get; set; }
    public AccountType AccountType { get; set; }
    public int? AccountingRegulationId { get; set; }

    public int? AccountBehavior { get; set; } = 0; //0: normal, 1: 2-sides closing balance

    [ForeignKey(nameof(AccountingRegulationId))]
    public AccountingRegulation? AccountingRegulation { get; set; }
}

