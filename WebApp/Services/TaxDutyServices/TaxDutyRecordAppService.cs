using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Linq;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.TaxDutyServices.Dto;
using WebApp.Services.UserService;

namespace WebApp.Services.TaxDutyServices;

public interface ITaxDutyRecordAppService
{
    Task<ResponseEntity> CreateDutyRecord(TaxDutyRecordDto input);
    Task<ResponseEntity> CreateDutyRecordsForOrganization(string period);
    Task<ResponseEntity> CreateDutyRecordsForOrganizations(string period);
    Task<ResponseEntity> UpdateTaxDutyRecord(string id, TaxDutyRecordDto input);
    Task<ResponseEntity> DeleteTaxDutyRecord(string id);
}

public class TaxDutyRecordAppService(IUserManager userManager,
                                     ILogger<TaxDutyRecordAppService> logger,
                                     AppDbContext dbContext) : BaseAppService(userManager), ITaxDutyRecordAppService
{
    public async Task<ResponseEntity> CreateDutyRecord(TaxDutyRecordDto input)
    {
        var workingOrg = await dbContext.Organizations
                                        .CountAsync(x => x.Id == WorkingOrg.ToGuid());
        if (workingOrg <= 0) return ResponseEntity.Error404("Working organization not found");
        var taxDuty = await dbContext.TaxReportDuties
                                     .FirstOrDefaultAsync(x => x.Id == input.TaxDutyId);
        if (taxDuty is null) return ResponseEntity.Error404("Tax duty not found");
        var newRecord = new TaxDutyRecord
        {
            OrganizationId = WorkingOrg.ToGuid(),
            TaxDutyId = input.TaxDutyId,
            DueDate = input.DueDate,
            DutyPeriodType = input.DutyPeriodType,
            Period = input.Period,
            Status = input.Status,
            PaymentStatus = input.PaymentStatus,
            TaxPayable = input.TaxPayable,
            Note = input.Note
        };
        await dbContext.AddAsync(newRecord);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(newRecord);
    }

    public async Task<ResponseEntity> CreateDutyRecordsForOrganization(string period)
    {
        var duties = await dbContext.OrganizationTaxDuties
                                    .Where(x => x.OrganizationId == WorkingOrg.ToGuid())
                                    .ToListAsync();
        var periodValue = period.Split("-")[0].ToInt();
        var periodYear = period.Split("-")[1].ToInt();
        if (duties.Count == 0)
            return ResponseEntity.Error404("Current organization has no tax duty");
        List<TaxDutyRecord> records = [];

        records.AddRange(duties.Select(duty => new TaxDutyRecord()
        {
            OrganizationId = WorkingOrg.ToGuid(),
            TaxDutyId = duty.TaxReportDutyId,
            DutyPeriodType = duty.DutyPeriodType,
            DueDate = duty.DutyPeriodType switch
            {
                DutyPeriodType.Monthly => new DateTime(periodYear, periodValue, 1).AddMonths(1).AddDays(19),
                DutyPeriodType.Quarterly => StringConverter.GetQuarterEndDate(periodValue, periodYear),
                DutyPeriodType.Annual => new DateTime(periodYear, 12, 31).AddDays(90),
                _ => throw new NotImplementedException()
            },
            PaymentStatus = TaxPaymentStatus.Unpaid,
            Status = DutyStatus.Unreported,
            TaxPayable = 0,
            Period = period,
            Note = null
        }));

        await dbContext.AddRangeAsync(records);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> CreateDutyRecordsForOrganizations(string period)
    {
        var periodSplit = period.Split("/");
        var periodValue = periodSplit[0].ToInt();
        var periodYear = periodSplit[1].ToInt();
        var organizations = await dbContext.Users.Where(x => x.Id == UserId.ToGuid())
                                           .SelectMany(x => x.Organizations.Select(o => o.Id))
                                           .ToListAsync();
        List<TaxDutyRecord> records = [];
        foreach (var org in organizations)
        {
            var duties = await dbContext.OrganizationTaxDuties
                                        .Where(x => x.OrganizationId == org)
                                        .ToListAsync();
            if (duties.Count == 0) continue;
            var existingRecords = await dbContext.TaxDutyRecords
                                                 .AnyAsync(x => x.OrganizationId == org 
                                                                && x.Period == period 
                                                                && !x.Deleted);
            if (existingRecords) continue;
            records.AddRange(duties.Select(duty => new TaxDutyRecord()
            {
                OrganizationId = WorkingOrg.ToGuid(),
                TaxDutyId = duty.TaxReportDutyId,
                DutyPeriodType = duty.DutyPeriodType,
                DueDate = duty.DutyPeriodType switch
                {
                    DutyPeriodType.Monthly => new DateTime(periodYear, periodValue, 1).AddMonths(1).AddDays(19),
                    DutyPeriodType.Quarterly => StringConverter.GetQuarterEndDate(periodValue, periodYear),
                    DutyPeriodType.Annual => new DateTime(periodYear, 12, 31).AddDays(90),
                    _ => throw new InvalidActionException("Invalid duty period type.")
                },
                PaymentStatus = TaxPaymentStatus.Unpaid,
                Status = DutyStatus.Unreported,
                TaxPayable = 0,
                Period = period,
                Note = null
            }));
        }
        await dbContext.AddRangeAsync(records);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult($"Successfully created {records.Count} records.");
    }

    public async Task<ResponseEntity> UpdateTaxDutyRecord(string id, TaxDutyRecordDto input)
    {
        var found = await dbContext.TaxDutyRecords.FindAsync(id);
        if (found is null) return ResponseEntity.Error404("Tax duty record not found");
        found.Status = input.Status;
        found.PaymentStatus = input.PaymentStatus;
        found.TaxPayable = input.TaxPayable;
        found.Note = input.Note ?? found.Note;
        dbContext.UpdateRange(found);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> DeleteTaxDutyRecord(string id)
    {
        var result = await dbContext.TaxDutyRecords.DeleteByKeyAsync(id);
        return result > 0 
            ? ResponseEntity.OkResult("Deleted successfully.") 
            : ResponseEntity.Error404("Tax duty record not found.");
    }
}