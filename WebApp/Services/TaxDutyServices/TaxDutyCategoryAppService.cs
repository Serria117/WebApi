using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.TaxDutyServices.Dto;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.TaxDutyServices;

public interface ITaxDutyCategoryAppService
{
    Task CreateTaxDutyCategory(TaxDutyCategoryDto input);
    Task<ResponseEntity> FindAll(RequestParam req);
    Task<ResponseEntity> FindById(int id);
    Task<ResponseEntity> UpdateTaxDutyCategory(int id, TaxDutyCategoryDto input);
    Task<ResponseEntity> DeleteTaxDutyCategory(int id);
}

public class TaxDutyCategoryAppService(AppDbContext dbContext) : ITaxDutyCategoryAppService
{
    public async Task CreateTaxDutyCategory(TaxDutyCategoryDto input)
    {
        var category = new TaxDutyCategory()
        {
            Name = input.Name,
            Code = input.Code,
            UnsignName = input.Name.UnSign()
        };
        await dbContext.AddAsync(category);
        await dbContext.SaveChangesAsync();
    }

    public async Task<ResponseEntity> FindAll(RequestParam req)
    {
        req.Valid();
        var query = dbContext.TaxDutyCategories
                             .Where(x => !x.Deleted)
                             .OrderBy($"{req.SortBy} {req.OrderBy}")
                             .AsNoTracking();
        
        // Filter by keyword if available
        if (!string.IsNullOrEmpty(req.Keyword))
        {
            var keyword = req.Keyword.UnSign();
            query = query.Where(x => x.UnsignName.Contains(keyword)
                                     || x.Code.Contains(keyword));
        }

        // Paging if available
        if (req is { Page: not null, Size: not null })
        {
            var resultPage = await query.ToPagedListAsync(req.Page.Value, req.Size.Value);
            return ResponseEntity.OkResult(resultPage.Select(x => new TaxDutyCategoryDisplayDto
            {
                Id = x.Id, Name = x.Name, Code = x.Code
            }));
        }
        
        //Non paging
        var result = await query.ToListAsync();
        return ResponseEntity.OkResult(result.Select(x => new TaxDutyCategoryDisplayDto
        {
            Id = x.Id, Name = x.Name, Code = x.Code
        }));
    }

    public async Task<ResponseEntity> FindById(int id)
    {
        var found = await dbContext.TaxDutyCategories
                                   .Include(x => x.TaxReportDuties)
                                   .AsSplitQuery()
                                   .AsNoTracking()
                                   .Select(x => new TaxDutyCategoryDisplayDto
                                   {
                                       Id = x.Id, Name = x.Name, Code = x.Code,
                                       TaxDutyRecords = x.TaxReportDuties.Select(d => new TaxDutyDisplayDto
                                       {
                                           Id = d.Id,
                                           Name = d.Name,
                                           Code = d.Code,
                                           Description = d.Description
                                       }).ToList()
                                   })
                                   .FirstOrDefaultAsync(x => x.Id == id);
        
        return found == null 
            ? ResponseEntity.Error404("Id Not Found") 
            : ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> UpdateTaxDutyCategory(int id, TaxDutyCategoryDto input)
    {
        var found = await dbContext.TaxDutyCategories
                                   .FirstOrDefaultAsync(x => x.Id == id);
        if (found == null) return ResponseEntity.Error404("Id Not Found");
        found.Name.UpdateNotNull(input.Name.RemoveSpace());
        found.Code.UpdateNotNull(input.Code.RemoveSpace());
        found.UnsignName = found.Name.UnSign();
        await dbContext.SaveChangesAsync();
        return ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> DeleteTaxDutyCategory(int id)
    {
        var found = await dbContext.TaxDutyCategories
                                   .Include(x => x.TaxReportDuties)
                                   .AsSplitQuery()
                                   .FirstOrDefaultAsync(x => x.Id == id);
        if (found == null) return ResponseEntity.Error404("Id Not Found");
        found.Deleted = true;
        foreach (var duty in found.TaxReportDuties)
        {
            duty.Deleted = true;
        }
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }
}