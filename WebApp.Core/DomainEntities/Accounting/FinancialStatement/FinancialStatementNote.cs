using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Accounting.FinancialStatement;

[Table("ACC_FinancialStatementNote")]
public class FinancialStatementNote : BaseEntity<int>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public int RegulationId { get; set; }

    [MaxLength(255)]
    public string TemplateFile { get; set; } = string.Empty;

}