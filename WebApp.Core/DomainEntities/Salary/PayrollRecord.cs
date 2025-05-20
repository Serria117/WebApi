using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Salary;

[Index(nameof(EmployeeId), nameof(PayrollPeriodId), IsUnique = true)]
public class PayrollRecord : BaseEntityAuditable<long>
{
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalGrossPay { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalNetPay { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public required Employee Employee { get; set; } //navigation property

    public Guid EmployeeId { get; set; } // Foreign key

    [ForeignKey(nameof(PayrollPeriodId))]
    public required PayrollPeriod PayrollPeriod { get; set; } //navigation property

    public int PayrollPeriodId { get; set; } // Foreign key
}