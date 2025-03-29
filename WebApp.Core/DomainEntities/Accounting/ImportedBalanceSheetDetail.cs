using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using WebApp.Enums;

namespace WebApp.Core.DomainEntities.Accounting;

[Index(nameof(Account))]
public class ImportedBalanceSheetDetail : BaseEntity<int>
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

    public bool? IsValid { get; set; }

    [MaxLength(255)]
    public string? Note { get; set; }
    
    public ImportedBalanceSheet ImportedBalanceSheet { get; set; } = null!;

    public override string ToString()
    {
        return $"TK: {Account} " +
               $"- Dk_No: {OpenCredit} - Dk_Co: {OpenDebit} " +
               $"- Ps_No: {AriseCredit} - Ps_Co: {AriseDebit} " +
               $"- Ck_No: {CloseCredit} - Cs_Co: {CloseDebit}";
    }
}