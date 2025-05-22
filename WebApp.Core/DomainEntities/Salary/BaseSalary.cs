using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Salary;

public class BaseSalary : BaseEntityAuditable<int>
{
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    public Guid EmployeeId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentSalary { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? LastSalary { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}