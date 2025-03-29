using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting;

public class BalanceSheetEntry : BaseEntityAuditable<int>
{
    public int BalanceSheetId { get; set; }
    public Account Account { get; set; } = null!;
    public int AccountId { get; set; }

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
}