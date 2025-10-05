using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;
[Table(name: "ACC_IncomeStatementMap")]
public class IncomeStatementMap : BaseEntity<int>
{
    public Account Account { get; set; } = null!;
    public IncomeStatementItem IncomeStatementItem { get; set; } = null!;
    public int RegulationId { get; set; }
}