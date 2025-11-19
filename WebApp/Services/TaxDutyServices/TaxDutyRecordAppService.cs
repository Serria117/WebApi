using Microsoft.EntityFrameworkCore;
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
    Task<ResponseEntity> CreateDutyRecordsForOrganizations(TaxDutyRecordsCreateDto input);
    Task<ResponseEntity> UpdateTaxDutyRecord(string id, TaxDutyRecordDto input);
    Task<ResponseEntity> DeleteTaxDutyRecord(string id);
    Task<ResponseEntity> FindTaxDutyRecords(int? year, 
                                            DutyPeriodType? periodType, 
                                            string? period, 
                                            DutyStatus? status,
                                            Guid? orgId,
                                            string? keyword);

    Task<ResponseEntity> UpdateTaxDutyRecords(ICollection<TaxDutyRecordUpdateDto> dtos);
}

public class TaxDutyRecordAppService(IUserManager userManager,
                                     ILogger<TaxDutyRecordAppService> logger,
                                     AppDbContext dbContext) : BaseAppService(userManager), ITaxDutyRecordAppService
{
    public async Task<ResponseEntity> FindTaxDutyRecords(int? year,
                                                         DutyPeriodType? periodType,
                                                         string? period,
                                                         DutyStatus? status,
                                                         Guid? orgId,
                                                         string? keyword)
    {
        //Get all the organizations that belong to the current user.
        var organizationIds = await dbContext.Users
                                             .Where(u => u.Id == UserId.ToGuid())
                                             .SelectMany(u => u.Organizations.Select(o => o.Id))
                                             .ToHashSetAsync();
        keyword = keyword.RemoveSpace();
        var filterYear = year ?? DateTime.Now.Year; //force year to be current year if it is not provided
        var filterKeyword = keyword ?? string.Empty;
        var query = dbContext.TaxDutyRecords
                             .Where(x => x.Period.EndsWith(filterYear.ToString()))
                             .AsQueryable();
        
        if (status != null)
            query = query.Where(x => x.Status == status); //Filter with status
        if (orgId is not null && orgId != Guid.Empty)
            query = query.Where(x => x.OrganizationId == orgId); //Filter with organization
        if (periodType is not null)
            query = query.Where(x => x.DutyPeriodType == periodType);
        if (!string.IsNullOrEmpty(period))
            query = query.Where(x => x.Period == period); //Filter with period after period type filtering

        var duties = await query.Join(dbContext.TaxReportDuties.Where(t => !t.Deleted),
                                      d => d.TaxDutyId, t => t.Id,
                                      (duty, report) => new
                                      {
                                          Duty = duty,
                                          Report = report
                                      })
                                .Join(dbContext.Organizations
                                               .Where(o => organizationIds.Contains(o.Id))
                                               .Where(o => o.UnsignName.Contains(filterKeyword)),
                                      rd => rd.Duty.OrganizationId, o => o.Id,
                                      (rd, o) => new
                                      {
                                          Report = rd.Report,
                                          Duty = rd.Duty,
                                          Org = o
                                      })
                                .AsNoTracking()
                                .AsSplitQuery()
                                .GroupBy(x => x.Org.Id)
                                .Select(g => new
                                {
                                    OrganizationId = g.Key,
                                    g.First().Org.FullName,
                                    g.First().Org.ShortName,
                                    g.First().Org.TaxId,
                                    TotalTaxPayable = g.Sum(x => x.Duty.TaxPayable),
                                    TaxDuties = g.Select(x => new
                                    {
                                        x.Duty.Id,
                                        ReportName = x.Report.Name,
                                        ReportId = x.Report.Id,
                                        x.Duty.Period,
                                        x.Duty.Status,
                                        x.Duty.TaxPayable,
                                        x.Duty.DutyPeriodType,
                                        x.Duty.PaymentStatus,
                                        x.Duty.DueDate
                                    }).OrderBy(x => x.ReportId).ToList()
                                }).ToListAsync();


        return ResponseEntity.OkResult(duties);
    }

    public async Task<ResponseEntity> UpdateTaxDutyRecords(ICollection<TaxDutyRecordUpdateDto> dtos)
    {
        var duties = await dbContext.TaxDutyRecords
                                    .Where(x => dtos.Select(d => d.Id).Contains(x.Id))
                                    .ToListAsync();
        if (duties.Count == 0) return ResponseEntity.Error404("No duty found");
        int unchangedCount = 0;
        foreach (var dto in dtos)
        {
            var duty = duties.FirstOrDefault(x => x.Id == dto.Id);
            if (duty is null) continue;

            //Remove from list if nothing changed
            if (duty.PaymentStatus == dto.PaymentStatus &&
                duty.TaxPayable == dto.TaxPayable &&
                dto.Status == duty.Status)
            {
                duties.Remove(duty);
                unchangedCount++;
                continue;
            }

            duty.PaymentStatus = dto.PaymentStatus;
            duty.TaxPayable = dto.TaxPayable;
            duty.Status = dto.Status;
        }

        await dbContext.BulkUpdateAsync(duties); //Bulk update duties
        return ResponseEntity.OkResult($"Cập nhật thành công {duties.Count}/{dtos.Count} bản ghi. " +
                                       $"{unchangedCount} bản ghi không có thay đổi.");
    }

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

    public async Task<ResponseEntity> CreateDutyRecordsForOrganizations(TaxDutyRecordsCreateDto input)
    {
        var periodValue = 12;
        int periodYear;
        if (input.Period.Contains('/'))
        {
            var periodSplit = input.Period.Split('/');
            periodValue = periodSplit[0].ToInt();
            periodYear = periodSplit[1].ToInt();
        }
        else
        {
            periodYear = input.Period.ToInt();
        }

        var organizations = await dbContext.Users.Where(x => x.Id == UserId.ToGuid())
                                           .SelectMany(x => x.Organizations
                                                             .Select(o => o
                                                                         .Id)) //get all the organizations of current user
                                           .Where(o => input.Organizations.Contains(o)) //filter only the selected ones
                                           .ToListAsync();
        List<TaxDutyRecord> records = [];
        foreach (var org in organizations)
        {
            var duties = await dbContext.OrganizationTaxDuties
                                        .Where(x => x.OrganizationId == org
                                                    && x.DutyPeriodType ==
                                                    input.DutyPeriodType //filter by duty period type
                                                    && !x.Deleted)
                                        .ToListAsync();
            if (duties.Count == 0) continue;
            var existingRecords = await dbContext.TaxDutyRecords
                                                 .AnyAsync(x => x.OrganizationId == org
                                                                && x.Period == input.Period
                                                                && !x.Deleted);
            if (existingRecords) continue;
            records.AddRange(duties.Select(duty => new TaxDutyRecord
            {
                OrganizationId = org,
                TaxDutyId = duty.TaxReportDutyId,
                DutyPeriodType = duty.DutyPeriodType,
                DueDate = duty.DutyPeriodType switch
                {
                    DutyPeriodType.Monthly => new DateTime(periodYear, periodValue, 1).AddMonths(1).AddDays(19),
                    DutyPeriodType.Quarterly => StringConverter.GetQuarterEndDate(periodValue, periodYear).AddMonths(1),
                    DutyPeriodType.Annual => new DateTime(periodYear, 12, 31).AddDays(90),
                    DutyPeriodType.Other => DateTime.Now.AddDays(10),
                    _ => throw new InvalidActionException("Invalid duty period type.")
                },
                PaymentStatus = TaxPaymentStatus.Unpaid,
                Status = DutyStatus.Unreported,
                TaxPayable = 0,
                Period = input.Period,
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