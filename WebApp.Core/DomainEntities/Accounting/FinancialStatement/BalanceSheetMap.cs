using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table(name: "ACC_BalanceSheetMaps")]
public class BalanceSheetMap : BaseEntity<int>
{
    public int RegulationId { get; set; }
    public int BalanceSheetItemId { get; set; }
    public int? AccountId { get; set; }

    [ForeignKey("BalanceSheetItemId")]
    public BalanceSheetItem BalanceSheetItem { get; set; } = null!;

    [ForeignKey("AccountId")]
    public Account? Account { get; set; }

    public ValueType ValueType { get; set; } //Determine whether to take value from Debit or Credit side of the account

    public bool NegativeValue { get; set; } = false; //Determine if the value should be negative
}

public enum ValueType
{
    Debit = 0, //Take value from Debit side of the account
    Credit = 1 //Take value from Credit side of the account
}