using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

[Table("PR_Salary")]
public class Salary : BaseEntity<long>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal SalaryValue { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? InsuranceSalaryValue { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!; // Navigation property

    public long EmployeeId { get; set; } // Foreign key to Employee entity
}