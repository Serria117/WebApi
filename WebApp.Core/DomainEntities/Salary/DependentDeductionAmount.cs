using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class DependentDeductionAmount : BaseEntityAuditable<int>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; } // nullable for future updates
}