using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Enums;

namespace WebApp.Core.DomainEntities.Accounting;

public class BalanceSheetDetail : BaseEntity<int>
{
    [MaxLength(10)]
    public string? Account { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpenCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal OpenDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AriseCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AriseDebit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CloseCredit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CloseDebit { get; set; }

    [MaxLength(5)]
    public string? Parent { get; set; }

    [MaxLength(5)]
    public string? B01TS { get; set; }

    [MaxLength(5)]
    public string? B01NV { get; set; }

    [MaxLength(5)]
    public string? B02 { get; set; }

    public BalanceSheet? BalanceSheet { get; set; }
}