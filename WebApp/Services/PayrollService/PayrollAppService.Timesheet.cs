using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task<AppResponse> CreateTimesheetsAsync(long periodId)
    {
        var payrollPeriod = await GetPayrollPeriod(periodId);
        if (payrollPeriod is null) return AppResponse.Error404("PayrollPeriod not found.");

        var timesheets = await CreateTimesheetsForPeriod(payrollPeriod);

        return AppResponse.OkResult(timesheets.Select(t => new
        {
            t.Id,
            t.Date,
            t.Value,
            t.IsWorkDay,
            t.IsLeave,
            t.LeaveType,
            t.IsTripDay,
            t.EmployeeId,
            t.PayrollPeriodId
        }).ToList());
    }

    public async Task<AppResponse> UpdateTimesheetAsync(TimesheetUpdate dto)
    {
        var timesheet = await TimesheetRepository.Find(t => t.Id == dto.Id
                                                            && t.OrganizationId == WorkingOrg.ToGuid())
                                                 .FirstOrDefaultAsync();
        if (timesheet is null) throw new KeyNotFoundException("Timesheet not found.");

        timesheet.Value = dto.Value;
        timesheet.IsWorkDay = dto.IsWorkDay;
        timesheet.IsLeave = dto.IsLeave;
        timesheet.LeaveType = dto.LeaveType;
        timesheet.IsHoliday = dto.IsHoliday;
        timesheet.IsTripDay = dto.IsTripDay;

        await TimesheetRepository.UpdateAsync(timesheet);
        return AppResponse.OkResult(new TimesheetDisplay
        {
            Id = timesheet.Id,
            Date = timesheet.Date,
            Value = timesheet.Value,
            IsWorkDay = timesheet.IsWorkDay,
            IsLeave = timesheet.IsLeave,
            LeaveType = timesheet.LeaveType,
            IsTripDay = timesheet.IsTripDay,
            EmployeeId = timesheet.EmployeeId,
            PayrollPeriodId = timesheet.PayrollPeriodId
        });
    }

    public async Task DeleteTimesheetInPeriodAsync(PayrollPeriod payrollPeriod)
    {
        var timesheets = await TimesheetRepository.Find(t => t.PayrollPeriodId == payrollPeriod.Id)
                                                  .Select(t => t.Id)
                                                  .ToListAsync();
        if (timesheets.Count > 0)
            await TimesheetRepository.HardDeleteManyAsync(timesheets);
    }

    private async Task<List<Timesheet>> CreateTimesheetsForPeriod(PayrollPeriod payrollPeriod)
    {
        List<Timesheet> timesheets = [];

        var employees = await EmployeeRepository
                              .Find(e => e.OrganizationId == WorkingOrg.ToGuid()
                                         && !e.Deleted &&
                                         e.JoinDate <= payrollPeriod.StartDate &&
                                         (e.EndDate == null || e.EndDate >= payrollPeriod.EndDate))
                              .ToListAsync();

        foreach (var employee in employees)
        {
            for (var date = payrollPeriod.StartDate; date <= payrollPeriod.EndDate; date = date.AddDays(1))
            {
                var isWorkDay = true;
                switch (payrollPeriod.Weekend)
                {
                    case Weekend.Sunday:
                        if (date.DayOfWeek is DayOfWeek.Sunday)
                        {
                            isWorkDay = false;
                        }

                        break;
                    case Weekend.SundayAndSaturday:
                        if (date.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
                        {
                            isWorkDay = false;
                        }

                        break;
                    default:
                        isWorkDay = true;
                        break;
                }

                var value = isWorkDay ? 1m : 0m;

                var timesheet = new Timesheet
                {
                    Id = Ulid.NewUlid().ToString(),
                    Date = date,
                    Value = value,
                    IsWorkDay = isWorkDay,
                    IsLeave = false, // Assuming no leave by default, can be modified later
                    EmployeeId = employee.Id,
                    OrganizationId = payrollPeriod.OrganizationId,
                    PayrollPeriodId = payrollPeriod.Id
                };

                timesheets.Add(timesheet);
            }
        }

        if (timesheets.Count > 0)
        {
            await TimesheetRepository.CreateManyAsync(timesheets);
        }

        return timesheets;
    }

}