using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table(name: "ACC_IncomeStatements")]
public class IncomeStatement : BaseEntity<int>
{
    public ICollection<IncomeStatementEntry> Entries { get; set; } = [];
}