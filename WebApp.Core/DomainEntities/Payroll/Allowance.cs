using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;
[Table("PR_Allowance")]
public class Allowance : BaseEntityAuditable<long>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long AllowanceTypeId { get; set; }
    public long? EmployeeId { get; set; }

    [ForeignKey(nameof(AllowanceTypeId))]
    public AllowanceType AllowanceType { get; set; } = null!;

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; } = null!;
}