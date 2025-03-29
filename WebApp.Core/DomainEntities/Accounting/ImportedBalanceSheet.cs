using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Accounting;

[Index(nameof(CreateAt))]
public class ImportedBalanceSheet : BaseEntityAuditable<int>
{
    [MaxLength(50)]
    public string? Name { get; set; }

    public BalanceSheet? BalanceSheet { get; set; }
    public int? Year { get; set; }
    public int? BalanceSheetId { get; set; }
    public HashSet<ImportedBalanceSheetDetail> Details { get; set; } = [];
    public Organization? Organization { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumOpenCredit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumOpenDebit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumAriseCredit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumAriseDebit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumCloseCredit { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal SumCloseDebit { get; set; } = 0;
    
    public bool IsValid { get; set; } = true;

    public override string ToString()
    {
        return $"{Name} - ({Year}):\n" +
               $"Open: CR = {SumOpenCredit} - DR = {SumOpenDebit}\n" +
               $"Arise: CR = {SumAriseCredit} - DR = {SumAriseDebit}\n" +
               $"Close: CR = {SumCloseCredit} - DR = {SumCloseDebit}\n";
    }
}