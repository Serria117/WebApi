using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task<ResponseBase> CreatePayrollPeriodsAsync(int year, Weekend weekend)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1990);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, DateTime.Now.Year);

        var org = await OrganizationRepository.Find(o => o.Id == WorkingOrg.ToGuid())
                                              .FirstOrDefaultAsync();
        if (org is null) return ResponseBase.Error404("Organization not found.");
        var previousVersion = await PayrollPeriodRepository
                                    .Find(p => !p.Deleted && p.OrganizationId == WorkingOrg.ToGuid())
                                    .Select(p => p.Version)
                                    .Distinct()
                                    .CountAsync();
        List<PayrollPeriod> periods = [];
        for (var i = 1; i <= 12; i++)
        {
            var startDate = new DateTime(year, i, 1);
            var endDate = new DateTime(year, i, DateTime.DaysInMonth(year, i));
            var period = new PayrollPeriod
            {
                Year = year,
                StartDate = startDate,
                EndDate = endDate,
                Organization = org,
                Weekend = weekend,
                TotalWorkDay = GetNetWorkingDay(startDate, endDate, weekend),
                Version = previousVersion + 1,
                Name = $"{org.TaxId}_BL_T{i:00}-{year}_v.{previousVersion + 1:00}",
            };
            periods.Add(period);
        }

        if (periods.Count > 0) await PayrollPeriodRepository.CreateManyAsync(periods);
        return ResponseBase.Ok();
    }

    public async Task<ResponseBase> GetPayrollPeriodAsync(long pId)
    {
        var result = await GetPayrollPeriod(pId);
        return result is null
            ? ResponseBase.Error404("PayrollPeriod not found.")
            : ResponseBase.OkResult(result);
    }

    // Retrieve the list of payroll periods
    public async Task<ResponseBase> GetYearPayrollPeriod(PayrollPeriodQuery query)
    {
        try
        {
            var periods = await PayrollPeriodRepository
                                .Find(p => !p.Deleted
                                           && p.OrganizationId == WorkingOrg.ToGuid()
                                           && (query.Year == null || p.Year == query.Year))
                                .OrderBy(p => p.Year).ThenBy(p => p.Version)
                                .ToListAsync();
            
            return ResponseBase.OkResult(periods.Select(x => x.ToDisplayDto()).ToList());
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return ResponseBase.Error(ResponseMessage.Error);
        }
    }
    
    private async Task<PayrollPeriod?> GetPayrollPeriod(long pId)
    {
        return await PayrollPeriodRepository
                     .Find(p => p.Id == pId && !p.Deleted)
                     .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Get total net working days between 2 date, depends on the <see cref="Weekend"/> type
    /// </summary>
    /// <param name="startDate">The starting date of the period</param>
    /// <param name="endDate">The ending date of the period</param>
    /// <param name="weekend">An enum represents the weekend type: Sunday only or Sunday and Saturday</param>
    /// <returns>The total net working days in the period</returns>
    private static int GetNetWorkingDay(DateTime startDate, DateTime endDate, Weekend weekend)
    {
        if (startDate > endDate) return 0;
        int workDays = 0;
        int totalDays = (endDate - startDate).Days + 1;
        switch (weekend)
        {
            case Weekend.Sunday:
                for (var i = 0; i < totalDays; i++)
                {
                    var currentDay = startDate.AddDays(i);
                    if (currentDay.DayOfWeek != DayOfWeek.Sunday)
                    {
                        workDays++;
                    }
                }
                break;
            case Weekend.SundayAndSaturday:
                for (var i = 0; i < totalDays; i++)
                {
                    var currentDay = startDate.AddDays(i);
                    if (currentDay.DayOfWeek != DayOfWeek.Sunday && currentDay.DayOfWeek != DayOfWeek.Saturday)
                    {
                        workDays++;
                    }
                }
                break;
            default:
                return totalDays; //return total day between 2 date if no weekend type defined
        }

        return workDays;
    }
}