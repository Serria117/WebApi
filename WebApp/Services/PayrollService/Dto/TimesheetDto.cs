using System.ComponentModel.DataAnnotations;
using WebApp.Core.DomainEntities.Payroll;

namespace WebApp.Services.PayrollService.Dto;

public class TimesheetCreate
{
    public DateTime Date { get; set; }

    /// <summary>
    /// The value of a working day. 1 if full day, 0.5 if half day or 0 if not a working day.
    /// </summary>
    [Range(0,1, ErrorMessage = "Value must be between 0 and 1.")]
    public decimal Value { get; set; }
    public bool IsWorkDay { get; set; } = true;
    public bool IsLeave { get; set; } = false;
    public LeaveType? LeaveType { get; set; }
    public bool IsTripDay { get; set; }
    public bool IsHoliday { get; set; }
    public long EmployeeId { get; set; }
    public Guid OrganizationId { get; set; }
    public long PayrollPeriodId { get; set; }
}

public class TimesheetUpdate : TimesheetCreate
{
    public string Id { get; set; } = string.Empty;
}

public class TimesheetDisplay
{
    public string Id { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public bool IsWorkDay { get; set; } = true;
    public bool IsLeave { get; set; } = false;
    public LeaveType? LeaveType { get; set; }
    public bool IsTripDay { get; set; }
    public long EmployeeId { get; set; }
    public long PayrollPeriodId { get; set; }
}