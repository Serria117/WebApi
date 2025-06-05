using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities.Salary;

[Index(nameof(EmployeeId), nameof(PayrollPeriodId), IsUnique = true)]
public class PayrollRecord : BaseEntityAuditable<long>
{
    [Column(TypeName = "decimal(18,2)")] [Range(1, 31)]
    public decimal ActualWorkDays { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalGrossPay { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalDeduction { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalNetPay { get; set; }

    public bool IsClosed { get; set; } = false;

    public TaxType TaxType { get; set; } = TaxType.ResidentProgressive;
    
    [ForeignKey(nameof(EmployeeId))]
    public required Employee Employee { get; set; } //navigation property

    public Guid EmployeeId { get; set; } // Foreign key

    [ForeignKey(nameof(PayrollPeriodId))]
    public PayrollPeriod PayrollPeriod { get; set; } = null!; //navigation property

    public int PayrollPeriodId { get; set; } // Foreign key

    public List<Timesheet> TimeSheets { get; set; } = []; //navigation property

    public List<PayrollItem> PayrollItems { get; set; } = []; //navigation property
}