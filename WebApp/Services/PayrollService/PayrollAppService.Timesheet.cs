using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.X509;
using WebApp.Core.DomainEntities.Salary;
using WebApp.Enums;
using WebApp.Enums.Payroll;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task<AppResponse> UpdateTimesheetsAsync(List<TimesheetUpdateDto> dto)
    {
        try
        {
            if (dto.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(dto), "Timesheet update data cannot be null");
            var timesheets = await TimesheetRepository.Find(x => dto.Select(d => d.Id).Contains(x.Id))
                                                      .ToListAsync();

            if (timesheets.IsNullOrEmpty())
            {
                throw new NotFoundException("No timesheets found for the provided IDs");
            }

            await transactionManager.BeginAsync();

            foreach (var timesheet in timesheets)
            {
                var updateDto = dto.FirstOrDefault(x => x.Id == timesheet.Id);

                updateDto?.UpdateEntity(timesheet);
            }

            var updateResult = await TimesheetRepository.UpdateManyAsync(timesheets, inTransaction: true);
            await transactionManager.CommitAsync();
            if (!updateResult.IsNullOrEmpty())
            {
                return AppResponse.Ok();
            }

            log.LogErrorFormatted("No timesheets were updated");
            return AppResponse.Error("No timesheets were updated");
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            await transactionManager.RollbackAsync();
            return AppResponse.Error500(ResponseMessage.Error);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    private async Task CreateTimesheetsAsync(PayrollPeriod payrollPeriod)
    {
        try
        {
            if (payrollPeriod is null) throw new NotFoundException("Payroll period not found");
            if (payrollPeriod.PayrollRecords.IsNullOrEmpty()) throw new NotFoundException("Payroll records not found");

            List<Timesheet> timesheets = [];
            await transactionManager.BeginAsync();
            WeekendType weekendType = payrollPeriod.WeekendType;
            var employeesInPeriod = payrollPeriod.PayrollRecords.Select(x => x.EmployeeId).ToList();
            foreach (var pr in payrollPeriod.PayrollRecords)
            {
                if (pr.IsClosed)
                {
                    throw new InvalidActionException($"Payroll period {payrollPeriod.Name} is already closed.");
                }


                for (var date = payrollPeriod.StartDate; date <= payrollPeriod.EndDate; date = date.AddDays(1))
                {
                    timesheets.AddRange(employeesInPeriod.Select(empId => new Timesheet
                    {
                        EmployeeId = empId,
                        Date = date,
                        PayrollRecord = pr,
                        Leave = LeaveType.NoLeave, // Default leave type
                        IsWeekend = weekendType switch
                        {
                            WeekendType.SaturdayAndSunday
                                => date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                            WeekendType.Sunday
                                => date.DayOfWeek == DayOfWeek.Sunday,
                            _
                                => false
                        },
                        IsHoliday = false, //TODO: This can be set based on a holiday calendar if available
                        IsTripDay = false, //TODO: This can be set based on trip records if available
                    }));
                }
            }

            if (timesheets.Count == 0)
            {
                throw new InvalidActionException("No payroll records found for the given period.");
            }

            await TimesheetRepository.CreateManyAsync(timesheets, inTransaction: true);
            await transactionManager.CommitAsync();
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogErrorFormatted(exception: e);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }
}