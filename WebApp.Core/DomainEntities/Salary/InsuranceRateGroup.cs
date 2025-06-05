using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities.Salary;

public class InsuranceRateGroup : BaseEntity<int>
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }

    public ICollection<InsuranceRate> InsuranceRates { get; set; } = new List<InsuranceRate>();
}