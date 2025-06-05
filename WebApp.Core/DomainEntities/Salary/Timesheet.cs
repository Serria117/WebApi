using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Enums.Payroll;

namespace WebApp.Core.DomainEntities.Salary;

public class Timesheet : BaseEntityAuditable<long>
{
    [ForeignKey(nameof(PayrollRecordId))]
    public PayrollRecord PayrollRecord { get; set; } = null!;

    public long PayrollRecordId { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the date of the timesheet entry.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the timesheet entry is for a weekend day.
    /// </summary>
    public bool IsWeekend { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the timesheet entry is for a holiday.
    /// </summary>
    public bool IsHoliday { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the timesheet entry is for a trip day.
    /// </summary>
    public bool IsTripDay { get; set; } = false;

    public Guid EmployeeId { get; set; }
    public Guid OrganizationId { get; set; }

    public LeaveType Leave { get; set; } = LeaveType.NoLeave;
}