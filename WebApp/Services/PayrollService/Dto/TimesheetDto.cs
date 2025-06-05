using System.ComponentModel.DataAnnotations;
using WebApp.Enums.Payroll;

namespace WebApp.Services.PayrollService.Dto;

public class TimesheetUpdateDto
{
    public long Id { get; set; }
    public bool IsHoliday { get; set; } = false;
    public bool IsTripDay { get; set; } = false;
    public LeaveType Leave { get; set; } = LeaveType.NoLeave;
    [MaxLength(255)]
    public string Description { get; set; } = string.Empty;
}