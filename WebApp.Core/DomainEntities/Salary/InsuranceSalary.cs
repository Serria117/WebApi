using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class InsuranceSalary : BaseEntityAuditable<int>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    public Guid EmployeeId { get; set; } // Foreign key

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; } // nullable for future updates
}