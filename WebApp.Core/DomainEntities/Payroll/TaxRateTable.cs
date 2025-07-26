using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_TaxRateTable")]
public class TaxRateTable: BaseEntity<long>
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public ICollection<TaxRate> TaxRates { get; set; } = new List<TaxRate>();
}