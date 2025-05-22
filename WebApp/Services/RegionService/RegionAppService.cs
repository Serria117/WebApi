using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.Mappers;
using WebApp.Services.RegionService.Dto;
using X.Extensions.PagedList.EF;
using X.PagedList;

namespace WebApp.Services.RegionService;

public interface IRegionAppService
{
    Task<AppResponse> CreateProvinceAsync(ProvinceCreateDto input);
    Task<AppResponse> CreateDistrictAsync(DistrictCreateDto input);
    Task<AppResponse> CreateTaxOfficeAsync(TaxOfficeCreateDto input);
    Task<AppResponse> GetAllProvincesAsync(PageRequest page);
    Task<AppResponse> GetProvinceAsync(int id);
    Task<AppResponse> GetDistrictsInProvinceAsync(int provinceId);
    Task<AppResponse> GetTaxOfficesInProvinceAsync(int provinceId);
    Task<AppResponse> CreateManyProvincesAsync(List<ProvinceCreateDto> input);
    Task<AppResponse> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input);
    Task<AppResponse> GetTaxOfficesByParentAsync(int parentId);
    Task<AppResponse> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input);
    Task<AppResponse> UpdateProvinceAsync(int id, ProvinceCreateDto input);
    Task<AppResponse> GetAllTaxOfficesAsync(PageRequest req);
    Task<AppResponse> FindTaxOfficeByIdAsync(int id);
    Task<AppResponse> FindTopLevelTaxOfficesAsync();
    Task<AppResponse> UpdateTaxOfficeAsync(int id, TaxOfficeCreateDto input);
    Task<bool> TaxOfficeCodeExists(string code);
}

public class RegionAppService(ILogger<RegionAppService> logger,
                              IAppRepository<Province, int> provinceRepo,
                              IAppRepository<District, int> districtRepo,
                              IAppRepository<TaxOffice, int> taxRepo) : IRegionAppService
{
    public async Task<AppResponse> CreateProvinceAsync(ProvinceCreateDto input)
    {
        var province = input.ToEntity();
        var saved = await provinceRepo.CreateAsync(province);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }

    public async Task<AppResponse> CreateManyProvincesAsync(List<ProvinceCreateDto> input)
    {
        try
        {
            var provinces = input.MapCollection(x => x.ToEntity()).ToList();
            await provinceRepo.CreateManyAsync(provinces);
            return new AppResponse();
        }
        catch (Exception e)
        {
            logger.LogError("Error:{message}",e.Message);
            logger.LogError("Stack trace: {stackTrace}", e.StackTrace);
            return AppResponse.Error("Failed to create provinces");
        }
    }

    public async Task<AppResponse> UpdateProvinceAsync(int id, ProvinceCreateDto input)
    {
        try
        {
            var found = await provinceRepo.FindByIdAsync(id);
            if (found is null) throw new NotFoundException("invalid province's ID");
            input.UpdateEntity(found);
            await provinceRepo.UpdateAsync(found);
            return AppResponse.Ok();
        }
        catch (Exception e)
        {
            logger.LogError("Failed to update province. Reason: {message}.", e.Message);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> CreateDistrictAsync(DistrictCreateDto input)
    {
        if (!await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId))
            return AppResponse.Error("Province could not be found");
        var district = input.ToEntity(provinceRepo);
        var saved = await districtRepo.CreateAsync(district);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }

    public async Task<AppResponse> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input)
    {
        try
        {
            if (!await provinceRepo.ExistAsync(x => x.Id == provinceId))
                return AppResponse.Error("Province could not be found");
            var districts = input.MapCollection(x => x.ToEntity(provinceRepo)).ToList();
            await districtRepo.CreateManyAsync(districts);
            return AppResponse.SuccessResponse("OK");
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}", e.Message);
            return AppResponse.Error("Failed to create districts");
        }
    }

    public async Task<AppResponse> CreateTaxOfficeAsync(TaxOfficeCreateDto input)
    {
        if(await TaxOfficeCodeExists(input.Code)) 
            return AppResponse.Error400($"Tax Office Code '{input.Code}' already exists");
        
        if (input.ProvinceId is not null && !await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId && !x.Deleted))
            return AppResponse.Error400("Province could not be found");
        
        // check for parent existence
        if (input.ParentId is not null && !await taxRepo.ExistAsync(x => x.Id == input.ParentId && !x.Deleted))
            return AppResponse.Error400("Parent Tax Office could not be found");
        
        var taxOffice = input.ToEntity(provinceRepo);
        
        var saved = await taxRepo.CreateAsync(taxOffice);
        return AppResponse.SuccessResponse(saved.ToDisplayDto());
    }
    
    public async  Task<AppResponse> UpdateTaxOfficeAsync(int id, TaxOfficeCreateDto input)
    {
        try
        {
            var found = await taxRepo.FindByIdAsync(id);
            if (found is null) throw new NotFoundException("Invalid tax office's ID");

            if (input.ParentId == id)
                throw new InvalidActionException("Invalid operation: Cannot set itself as its own parent!");

            //Check that the parent does not have this tax office as one of its children
            //This means we cannot set the parent to any of its children
            if (input.ParentId != null)
            {
                var children = await taxRepo.Find(x => x.ParentId != null && x.ParentId == found.Id)
                                            .Select(x => x.Id)
                                            .ToListAsync();

                if (children.Contains(input.ParentId.Value))
                {
                    throw new InvalidActionException(
                        $"Invalid operation: " +
                        $"Cannot set parent with ID '{input.ParentId}' " +
                        $"because it has this tax office as one of its children.");
                }
            }
            

            input.UpdateEntity(found);
            await taxRepo.UpdateAsync(found);
            return AppResponse.Ok();
        }
        catch (NotFoundException e)
        {
            return AppResponse.Error404(e.Message);
        }
        catch (InvalidActionException e)
        {
            return AppResponse.Error400(e.Message);
        }
        catch (Exception e)
        {
            return AppResponse.Error500(e.Message);
        }
    }

    public async Task<AppResponse> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input)
    {
        try
        {
            if (!await provinceRepo.ExistAsync(x => x.Id == pId))
                return AppResponse.Error("Province could not be found");
            var taxOffices = input.MapCollection(x => x.ToEntity(provinceRepo)).ToList();

            await taxRepo.CreateManyAsync(taxOffices);
            return AppResponse.SuccessResponse("OK");
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e.Message);
            return AppResponse.Error("Failed to create tax offices");
        }
    }
    
    public async Task<bool> TaxOfficeCodeExists(string code)
    {
        return await taxRepo.ExistAsync(x => x.Code == code && x.Deleted == false);
    }

    public async Task<AppResponse> GetAllTaxOfficesAsync(PageRequest req)
    {
        //Find the tax office without a parent, these are the top level tax offices.
        var parents = await taxRepo.Find(x => !x.Deleted && (x.ParentId == null || x.ParentId.Value == 0),
                                         sortBy: "Code", order: "ASC")
                                   .AsNoTracking()
                                   .ToPagedListAsync(req.Page, req.Size);

        var parentsDict = parents.ToDictionary(x => x.Id, x => x);

        //Get all child tax offices of each parent
        var allChildren = await taxRepo.Find(x => x.ParentId != null 
                                               && !x.Deleted
                                               && parentsDict.Keys.Contains(x.ParentId.Value),
                                          sortBy: "Code", order: "ASC")
                                    .AsNoTracking()
                                    .ToListAsync();

        var childrenByParent = allChildren.GroupBy(x => x.ParentId!.Value)
                                       .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var parent in parents)
        {
            if (childrenByParent.TryGetValue(parent.Id, out var childenOfParent))
            {
                parent.Children = [..childenOfParent];
            }
            else
            {
                parent.Children = [];
            }
        }

        return AppResponse.SuccessResponse(parents.MapPagedList(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> FindTaxOfficeByIdAsync(int id)
    {
        var taxOffice = await taxRepo.Find(filter: x => x.Id == id && !x.Deleted)
                                     .FirstOrDefaultAsync();

        if (taxOffice is null)
        {
            return AppResponse.Error404("Tax Office could not be found");
        }
        
        var children = await taxRepo.Find(x => x.ParentId != null && x.ParentId == id && !x.Deleted)
                                    .ToListAsync();
        taxOffice.Children = [..children];

        if (taxOffice.ParentId is { } or > 0)
        {
            var parent = await taxRepo.FindByIdAsync(taxOffice.ParentId.Value);
            taxOffice.Parent = parent;
        }
        
        return AppResponse.SuccessResponse(taxOffice.ToDisplayDto());
    }
    
    public async Task<AppResponse> FindTopLevelTaxOfficesAsync()
    {
        var topLevelTaxOffices = await taxRepo.Find(x => x.ParentId == null || x.ParentId.Value == 0).ToListAsync();
        return AppResponse.SuccessResponse(topLevelTaxOffices.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetAllProvincesAsync(PageRequest page)
    {
        var result = await provinceRepo.Find(x => !x.Deleted
                                                  && (string.IsNullOrWhiteSpace(page.Keyword) ||
                                                      x.Name.Contains(page.Keyword)),
                                             sortBy: page.SortBy, order: page.OrderBy,
                                             include: [nameof(Province.Districts), nameof(Province.TaxOffices)])
                                       .AsSplitQuery()
                                       .ToPagedListAsync(page.Page, page.Size);

        return AppResponse.SuccessResponse(result.MapPagedList(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetProvinceAsync(int id)
    {
        var province = await provinceRepo.Find(filter: x => x.Id == id && x.Deleted == false,
                                               include: nameof(Province.TaxOffices))
                                         .FirstOrDefaultAsync();
        return province == null
            ? AppResponse.Error404("Province could not be found")
            : AppResponse.SuccessResponse(province.ToDisplayDto());
    }

    public async Task<AppResponse> GetDistrictsInProvinceAsync(int provinceId)
    {
        var districts = await districtRepo.Find(filter: d => d.Province!.Id == provinceId,
                                                sortBy: "Id", order: "ASC")
                                          .ToListAsync();
        return AppResponse.SuccessResponse(districts.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetTaxOfficesInProvinceAsync(int provinceId)
    {
        var taxOffices = await taxRepo.Find(filter: t => t.Province != null &&  t.Province.Id == provinceId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return AppResponse.SuccessResponse(taxOffices.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<AppResponse> GetTaxOfficesByParentAsync(int parentId)
    {
        var taxOffices = await taxRepo.Find(x => x.ParentId != null && x.ParentId == parentId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return AppResponse.SuccessResponse(taxOffices.MapCollection(x => x.ToDisplayDto()));
    }
}