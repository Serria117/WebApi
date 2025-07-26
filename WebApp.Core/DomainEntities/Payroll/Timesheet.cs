using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Core.DomainEntities.Payroll;

/// <summary>
/// Represent a timesheet of a date
/// </summary>
[Table("PR_Timesheet")]
public class Timesheet : BaseEntity<string>
{
    public DateTime Date { get; set; }

    /// <summary>
    /// The value of a working day. 1 if full day, 0.5 if half day or 0 if not a working day.
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Value { get; set; }

    public bool IsWorkDay { get; set; } = true;
    public bool IsLeave { get; set; } = false;
    public bool IsHoliday { get; set; } = false;
    public LeaveType? LeaveType { get; set; }
    public bool IsTripDay { get; set; }
    public long EmployeeId { get; set; }

    public Guid OrganizationId { get; set; }
    public long PayrollPeriodId { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(PayrollPeriodId))]
    public PayrollPeriod PayrollPeriod { get; set; } = null!;
}

public enum LeaveType
{
    WithoutSalary,
    WithSalary
}