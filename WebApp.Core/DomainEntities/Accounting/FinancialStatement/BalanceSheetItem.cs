using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

/// <summary>
/// Represents an item in the balance sheet, such as assets or capital.
/// </summary>
[Table(name: "ACC_BalanceSheetItems")]
public class BalanceSheetItem : BaseEntity<int>
{
    [MaxLength(5)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public int RegulationId { get; set; }
    public AccountBalanceType AccountBalanceType { get; set; }

    [MaxLength(5)]
    public string? ParentCode { get; set; } = string.Empty;
}

public enum AccountBalanceType
{
    Asset,
    Capital
}