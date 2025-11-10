using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Services.TaxDutyServices.Dto;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.TaxDutyServices;

public interface ITaxDutyAppService
{
    Task<ResponseEntity> CreateTaxDuty(TaxDutyDto input);
    Task<ResponseEntity> FindTaxDuties(RequestParam req);
    Task<ResponseEntity> GetTaxDutyById(int id);
    Task<ResponseEntity> UpdateTaxDutyById(int id, TaxDutyDto input);
    Task<ResponseEntity> SoftDeleteTaxDutyById(int id);
    Task<ResponseEntity> AddOrUpdateTaxDutyForOrganization(OrganizationTaxDutiesDto input);
    Task<ResponseEntity> GetTaxDutiesByOrganizationId(Guid orgId);
}

public class TaxDutyAppService(IUserManager userManager,
                               AppDbContext dbContext,
                               ILogger<TaxDutyAppService> logger)
    : BaseAppService(userManager), ITaxDutyAppService
{
    public async Task<ResponseEntity> CreateTaxDuty(TaxDutyDto input)
    {
        var newTaxDuty = await dbContext.TaxReportDuties.AddAsync(new TaxReportDuty
        {
            Name = input.Name,
            Code = input.Code,
            Description = input.Description,
            Order = input.Order,
            TaxDutyCategoryId = input.CategoryId
        });

        return ResponseEntity.OkResult(new TaxDutyDisplayDto
        {
            Id = newTaxDuty.Entity.Id,
            Name = newTaxDuty.Entity.Name,
            Code = newTaxDuty.Entity.Code,
            Description = newTaxDuty.Entity.Description,
            Order = newTaxDuty.Entity.Order,
            CategoryId = input.CategoryId
        });
    }

    public async Task<ResponseEntity> FindTaxDuties(RequestParam req)
    {
        req.Valid();
        var taxDutiesQuery = dbContext.TaxReportDuties
                                      .Where(x => !x.Deleted)
                                      .OrderBy(x => x.TaxDutyCategoryId).ThenBy(x => x.Order)
                                      .AsNoTracking()
                                      .AsSplitQuery()
                                      .Select(x => new TaxDutyDisplayDto
                                      {
                                          Id = x.Id,
                                          Code = x.Code,
                                          Name = x.Name,
                                          Description = x.Description,
                                          Order = x.Order,
                                          CategoryId = x.TaxDutyCategoryId,
                                          CategoryName = x.Category.Name
                                      });
        if (!string.IsNullOrEmpty(req.Keyword))
        {
            taxDutiesQuery = taxDutiesQuery.Where(x => x.Name.Contains(req.Keyword)
                                                       || x.Code.Contains(req.Keyword)
                                                       || (x.Description != null &&
                                                           x.Description.Contains(req.Keyword)));
        }

        if (req is { Page: not null, Size: not null })
        {
            var resultPage = await taxDutiesQuery.ToPagedListAsync(req.Page.Value, req.Size.Value);
            return ResponseEntity.OkResult(resultPage);
        }

        var result = await taxDutiesQuery.ToListAsync();
        return ResponseEntity.OkResult(result);
    }

    public async Task<ResponseEntity> GetTaxDutyById(int id)
    {
        var found = await dbContext.TaxReportDuties.FindAsync(id);
        if (found == null || found.Deleted)
            return ResponseEntity.Error404("Entity not found or has been removed");
        return ResponseEntity.OkResult(new TaxDutyDisplayDto
        {
            Id = found.Id, Name = found.Name, Code = found.Code,
            Description = found.Description, Order = found.Order
        });
    }

    public async Task<ResponseEntity> UpdateTaxDutyById(int id, TaxDutyDto input)
    {
        var found = await dbContext.TaxReportDuties.FindAsync(id);
        if (found == null || found.Deleted)
            return ResponseEntity.Error404("Entity not found or has been removed");
        found.Name = input.Name;
        found.Code = input.Code;
        found.Description = input.Description;
        found.Order = input.Order;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> SoftDeleteTaxDutyById(int id)
    {
        var found = await dbContext.TaxReportDuties.FindAsync(id);
        if (found == null || found.Deleted)
            return ResponseEntity.Error404("Entity not found or has been removed");
        found.Deleted = true;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> AddOrUpdateTaxDutyForOrganization(OrganizationTaxDutiesDto input)
    {
        var organizationFound = await dbContext.Organizations
                                               .AnyAsync(x => x.Id == input.OrganizationId && !x.Deleted);

        if (!organizationFound)
            return ResponseEntity.Error404("Organization not found or has been deleted");

        // check duty exists and not added to the organization yet
        var existDuties = await dbContext.OrganizationTaxDuties
                                         .Where(x => x.OrganizationId == input.OrganizationId)
                                         .ToHashSetAsync();

        // get all existing duties from database
        var availableDuties = await dbContext.TaxReportDuties
                                             .Where(x => !x.Deleted)
                                             .Select(x => x.Id)
                                             .ToHashSetAsync();

        var validInputDutyLookup = input.Duties
                                         .Where(d => availableDuties.Contains(d.TaxReportDutyId))
                                         .GroupBy(d => d.TaxReportDutyId)
                                         .ToDictionary(group => group.Key, group => group.First());

        List<OrganizationTaxDuty> dutiesToAdd = [];
        List<OrganizationTaxDuty> dutiesToDelete = [];
        
        var existingDutyIds = existDuties.Select(x => x.TaxReportDutyId)
                                         .ToHashSet();

        // add new duties that are not existed before
        dutiesToAdd.AddRange(validInputDutyLookup.Values
                                      .Where(d => !existingDutyIds.Contains(d.TaxReportDutyId))
                                      .Select(d => new OrganizationTaxDuty
                                      {
                                          OrganizationId = input.OrganizationId,
                                          TaxReportDutyId = d.TaxReportDutyId,
                                          DutyPeriodType = d.DutyPeriodType
                                      }));

        dutiesToDelete.AddRange(existDuties.Where(e => !validInputDutyLookup.ContainsKey(e.TaxReportDutyId)));
        // remove duties that no longer needed
        existDuties.ExceptWith(dutiesToDelete);

        List<OrganizationTaxDuty> dutiesToUpdate = [];
        foreach (var duty in existDuties)
        {
            if (!validInputDutyLookup.TryGetValue(duty.TaxReportDutyId, out var inputDuty))
                continue;

            if (duty.DutyPeriodType != inputDuty.DutyPeriodType)
            {
                duty.DutyPeriodType = inputDuty.DutyPeriodType;
                dutiesToUpdate.Add(duty);
            }
        }

        if (dutiesToUpdate.Count + dutiesToAdd.Count + dutiesToDelete.Count == 0)
        {
            logger.LogInformation("No tax duties changed for organization with ID [{id}].", input.OrganizationId);
            return ResponseEntity.Ok();
        }
        dbContext.OrganizationTaxDuties.UpdateRange(dutiesToUpdate);
        dbContext.OrganizationTaxDuties.RemoveRange(dutiesToDelete);
        await dbContext.OrganizationTaxDuties.AddRangeAsync(dutiesToAdd);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> GetTaxDutiesByOrganizationId(Guid orgId)
    {
        var orgFound = await dbContext.Organizations.AnyAsync(x => x.Id == orgId && !x.Deleted);

        if (!orgFound) return ResponseEntity.Error404("Organization not found");

        var duties = await dbContext.OrganizationTaxDuties
                                    .Where(x => x.OrganizationId == orgId && !x.Deleted)
                                    .ToListAsync();

        return ResponseEntity.OkResult(duties);
    }
}