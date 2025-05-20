using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Enums;

namespace WebApp.Core.DomainEntities.Salary;

public class TimeSheet : BaseEntityAuditable<long>
{
    [ForeignKey(nameof(PayrollRecordId))]
    public required PayrollRecord PayrollRecord { get; set; }

    public long PayrollRecordId { get; set; }
    
    public string? Description { get; set; }
    public DateTime Date { get; set; }

    public bool IsWeekend { get; set; } = false;
    public bool IsHoliday { get; set; } = false;

    public Guid EmployeeId { get; set; }
    public Guid OrganizationId { get; set; }

    public LeaveType Leave { get; set; } = LeaveType.NoLeave;
}