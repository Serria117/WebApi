using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class InsuranceRate : BaseEntityAuditable<int>
{
    [MaxLength(20)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the rate of the insurance.
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Rate { get; set; } // Percentage rate for the insurance

    /// <summary>
    /// Gets or sets the value indicating whether the insurance is paid by the employee.
    /// </summary>
    public bool IsEmployeePaid { get; set; } = true;

    public int InsuranceRateGroupId { get; set; }
    
    [ForeignKey(nameof(InsuranceRateGroupId))]
    public InsuranceRateGroup InsuranceRateGroup { get; set; } = null!;
}