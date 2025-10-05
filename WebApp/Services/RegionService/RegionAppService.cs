using System.ComponentModel.DataAnnotations;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
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
    Task<ResponseBase> CreateProvinceAsync(ProvinceCreateDto input);
    Task<ResponseBase> CreateDistrictAsync(DistrictCreateDto input);
    Task<ResponseBase> CreateTaxOfficeAsync(TaxOfficeCreateDto input);
    Task<ResponseBase> GetAllProvincesAsync(PageRequest page);
    Task<ResponseBase> GetProvinceAsync(int id);
    Task<ResponseBase> GetDistrictsInProvinceAsync(int provinceId);
    Task<ResponseBase> GetTaxOfficesInProvinceAsync(int provinceId);
    Task<ResponseBase> CreateManyProvincesAsync(List<ProvinceCreateDto> input);
    Task<ResponseBase> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input);
    Task<ResponseBase> GetTaxOfficesByParentAsync(int parentId);
    Task<ResponseBase> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input);
    Task<ResponseBase> UpdateProvinceAsync(int id, ProvinceCreateDto input);
    Task<ResponseBase> GetAllTaxOfficesAsync(PageRequest req);
    Task<ResponseBase> FindTaxOfficeByIdAsync(int id);
    Task<ResponseBase> FindTopLevelTaxOfficesAsync();
    Task<ResponseBase> UpdateTaxOfficeAsync(int id, TaxOfficeCreateDto input);
    Task<bool> TaxOfficeCodeExists(string code);
    Task<ResponseBase> GetDistrictById(int id);
    Task<ResponseBase> CreateTaxOffice2(List<TaxOffice2CreateDto> dtos);
    Task<ResponseBase> GetTaxOffice2ByProvince(int provinceId);
    Task<ResponseBase> GetTaxOffice2ByDistrict(string districtCode, int provinceId);
    Task<ResponseBase> GetAllTaxOffice2(PageRequest req);
    Task<ResponseBase> GetTaxOffice2ById(int id);
    Task<ResponseBase> UpdateTaxOffice2(int id, TaxOffice2CreateDto input);
}

public class RegionAppService(ILogger<RegionAppService> logger,
                              IAppRepository<Province, int> provinceRepo,
                              IAppRepository<District, int> districtRepo,
                              IAppRepository<TaxOffice2, int> taxRepo2,
                              IAppRepository<TaxOffice, int> taxRepo) : IRegionAppService
{
    public async Task<ResponseBase> CreateProvinceAsync(ProvinceCreateDto input)
    {
        var province = input.ToEntity();
        var saved = await provinceRepo.CreateAsync(province);
        return ResponseBase.OkResult(saved.ToDisplayDto());
    }

    public async Task<ResponseBase> CreateManyProvincesAsync(List<ProvinceCreateDto> input)
    {
        try
        {
            var provinces = input.MapCollection(x => x.ToEntity()).ToList();
            await provinceRepo.CreateManyAsync(provinces);
            return new ResponseBase();
        }
        catch (Exception e)
        {
            logger.LogError("Error:{message}", e.Message);
            logger.LogError("Stack trace: {stackTrace}", e.StackTrace);
            return ResponseBase.Error("Failed to create provinces");
        }
    }

    public async Task<ResponseBase> UpdateProvinceAsync(int id, ProvinceCreateDto input)
    {
        try
        {
            var found = await provinceRepo.FindByIdAsync(id);
            if (found is null) throw new NotFoundException("invalid province's ID");
            input.UpdateEntity(found);
            await provinceRepo.UpdateAsync(found);
            return ResponseBase.Ok();
        }
        catch (Exception e)
        {
            logger.LogError("Failed to update province. Reason: {message}.", e.Message);
            return ResponseBase.Error(e.Message);
        }
    }

    public async Task<ResponseBase> CreateDistrictAsync(DistrictCreateDto input)
    {
        if (!await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId))
            return ResponseBase.Error("Province could not be found");
        var district = input.ToEntity(provinceRepo);
        var saved = await districtRepo.CreateAsync(district);
        return ResponseBase.OkResult(saved.ToDisplayDto());
    }

    public async Task<ResponseBase> CreateManyDistrictsAsync(int provinceId, List<DistrictCreateDto> input)
    {
        try
        {
            if (!await provinceRepo.ExistAsync(x => x.Id == provinceId))
                return ResponseBase.Error("Province could not be found");
            var districts = input.MapCollection(x => x.ToEntity(provinceRepo)).ToList();
            await districtRepo.CreateManyAsync(districts);
            return ResponseBase.OkResult("OK");
        }
        catch (Exception e)
        {
            logger.LogError("Error: {message}", e.Message);
            return ResponseBase.Error("Failed to create districts");
        }
    }

    public async Task<ResponseBase> GetDistrictById(int id)
    {
        var district = await districtRepo.Find(x => x.Id == id && !x.Deleted).FirstOrDefaultAsync();
        if (district is null)
        {
            return ResponseBase.Error404("District not found");
        }
        return ResponseBase.OkResult(district.ToDisplayDto());
    }

    public async Task<ResponseBase> CreateTaxOfficeAsync(TaxOfficeCreateDto input)
    {
        if (await TaxOfficeCodeExists(input.Code))
            return ResponseBase.Error400($"Tax Office Code '{input.Code}' already exists");

        if (input.ProvinceId is not null && !await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId && !x.Deleted))
            return ResponseBase.Error400("Province could not be found");

        // check for parent existence
        if (input.ParentId is not null && !await taxRepo.ExistAsync(x => x.Id == input.ParentId && !x.Deleted))
            return ResponseBase.Error400("Parent Tax Office could not be found");

        var taxOffice = input.ToEntity(provinceRepo);

        var saved = await taxRepo.CreateAsync(taxOffice);
        return ResponseBase.OkResult(saved.ToDisplayDto());
    }

    public async Task<ResponseBase> UpdateTaxOfficeAsync(int id, TaxOfficeCreateDto input)
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
            return ResponseBase.Ok();
        }
        catch (NotFoundException e)
        {
            return ResponseBase.Error404(e.Message);
        }
        catch (InvalidActionException e)
        {
            return ResponseBase.Error400(e.Message);
        }
        catch (Exception e)
        {
            return ResponseBase.Error500(e.Message);
        }
    }

    public async Task<ResponseBase> CreateManyTaxOfficeAsync(int pId, List<TaxOfficeCreateDto> input)
    {
        try
        {
            if (!await provinceRepo.ExistAsync(x => x.Id == pId))
                return ResponseBase.Error("Province could not be found");
            var taxOffices = input.MapCollection(x => x.ToEntity(provinceRepo)).ToList();

            await taxRepo.CreateManyAsync(taxOffices);
            return ResponseBase.OkResult("OK");
        }
        catch (Exception e)
        {
            logger.LogError("{message}", e.Message);
            return ResponseBase.Error("Failed to create tax offices");
        }
    }

    public async Task<bool> TaxOfficeCodeExists(string code)
    {
        return await taxRepo.ExistAsync(x => x.Code == code && x.Deleted == false);
    }

    public async Task<ResponseBase> GetAllTaxOfficesAsync(PageRequest req)
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
                parent.Children = [.. childenOfParent];
            }
            else
            {
                parent.Children = [];
            }
        }

        return ResponseBase.OkResult(parents.MapPagedList(x => x.ToDisplayDto()));
    }

    public async Task<ResponseBase> FindTaxOfficeByIdAsync(int id)
    {
        var taxOffice = await taxRepo.Find(filter: x => x.Id == id && !x.Deleted)
                                     .FirstOrDefaultAsync();

        if (taxOffice is null)
        {
            return ResponseBase.Error404("Tax Office could not be found");
        }

        var children = await taxRepo.Find(x => x.ParentId != null && x.ParentId == id && !x.Deleted)
                                    .ToListAsync();
        taxOffice.Children = [.. children];

        if (taxOffice.ParentId is { } or > 0)
        {
            var parent = await taxRepo.FindByIdAsync(taxOffice.ParentId.Value);
            taxOffice.Parent = parent;
        }

        return ResponseBase.OkResult(taxOffice.ToDisplayDto());
    }

    public async Task<ResponseBase> FindTopLevelTaxOfficesAsync()
    {
        var topLevelTaxOffices = await taxRepo.Find(x => x.ParentId == null || x.ParentId.Value == 0).ToListAsync();
        return ResponseBase.OkResult(topLevelTaxOffices.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<ResponseBase> GetAllProvincesAsync(PageRequest page)
    {
        var result = await provinceRepo.Find(x => !x.Deleted
                                                  && (string.IsNullOrWhiteSpace(page.Keyword) ||
                                                      x.Name.Contains(page.Keyword)),
                                             sortBy: page.SortBy, order: page.OrderBy)
                                       //.Include(p => p.Districts.Where(d => !d.Deleted))
                                       //.Include(p => p.TaxOffices.Where(t => !t.Deleted))
                                       .AsSplitQuery()
                                       .ToPagedListAsync(page.Page, page.Size);

        return ResponseBase.OkResult(result.MapPagedList(x => x.ToDisplayDto()));
    }

    public async Task<ResponseBase> GetProvinceAsync(int id)
    {
        var province = await provinceRepo.Find(filter: x => x.Id == id && x.Deleted == false)
                                         .Include(p => p.Districts.Where(d => !d.Deleted))
                                         //.Include(p => p.TaxOffices.Where(t => !t.Deleted))
                                         .FirstOrDefaultAsync();
        return province == null
            ? ResponseBase.Error404("Province could not be found")
            : ResponseBase.OkResult(province.ToDisplayDto());
    }

    public async Task<ResponseBase> GetDistrictsInProvinceAsync(int provinceId)
    {
        var districts = await districtRepo.Find(filter: d => d.Province!.Id == provinceId && !d.Deleted,
                                                sortBy: "Code", order: "ASC")
                                          .ToListAsync();
        if (districts.Count == 0) return ResponseBase.Error404("No districts found for the given province ID");
        return new ResponseBase
        {
            Data = districts.MapCollection(x => x.ToDisplayDto()).ToList(),
            Message = "OK",
            Code = "200",
            Success = true,
            TotalCount = districts.Count
        };

    }

    public async Task<ResponseBase> GetTaxOfficesInProvinceAsync(int provinceId)
    {
        var taxOffices = await taxRepo.Find(filter: t => t.Province != null && t.Province.Id == provinceId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return ResponseBase.OkResult(taxOffices.MapCollection(x => x.ToDisplayDto()));
    }

    public async Task<ResponseBase> GetTaxOfficesByParentAsync(int parentId)
    {
        var taxOffices = await taxRepo.Find(x => x.ParentId != null && x.ParentId == parentId,
                                            sortBy: "Id", order: "ASC")
                                      .AsNoTracking()
                                      .ToListAsync();
        return ResponseBase.OkResult(taxOffices.MapCollection(x => x.ToDisplayDto()));
    }

    #region New Tax Offices

    public async Task<ResponseBase> CreateTaxOffice2(List<TaxOffice2CreateDto> dtos)
    {
        List<TaxOffice2CreateDto> validDtos = [];
        foreach (var dto in dtos)
        {
            bool isValid = Validator.TryValidateObject(dto, new ValidationContext(dto), [], true);
            if (isValid)
            {
                validDtos.Add(dto);
            }
        }
        if (validDtos.Count == 0)
        {
            return ResponseBase.Error400("No valid tax office data");
        }
        var newTaxOffices = validDtos.Select(d => new TaxOffice2
        {
            FullName = d.FullName,
            ShortName = d.ShortName,
            Code = d.Code,
            ProvinceId = d.ProvinceId,
        }).ToList();
        await taxRepo2.CreateManyAsync(newTaxOffices);
        return new ResponseBase
        {
            Code = "200",
            Success = true,
            Data = new
            {
                SuccessCount = newTaxOffices.Count,
                FailedCount = dtos.Count - newTaxOffices.Count,
            },
            Message = $"{newTaxOffices.Count}/{dtos.Count} tax offices created successfully."
        };
    }

    public async Task<ResponseBase> GetTaxOffice2ByProvince(int provinceId)
    {
        var taxOffices = await taxRepo2.Find(x => x.ProvinceId == provinceId && !x.Deleted)
                                       .ToListAsync();
        return ResponseBase.OkResult(taxOffices.Select(x => new TaxOffice2DiplayDto
        {
            Id = x.Id,
            FullName = x.FullName,
            ShortName = x.ShortName,
            Code = x.Code,
            ProvinceId = x.ProvinceId,
            ProvinceName = x.Province?.Name
        }).ToList());
    }

    public async Task<ResponseBase> GetTaxOffice2ByDistrict(string districtCode, int provinceId)
    {
        var provinceTaxCode = districtCode[..3] + "00";
        var taxOffices = await taxRepo2.Find(x => (((x.Code == districtCode || x.Code == provinceTaxCode)
                                             && (x.ProvinceId == provinceId)) || x.ProvinceId == null)
                                             && !x.Deleted)
                                       .OrderBy(x => x.Code)
                                       .ToListAsync();
        return ResponseBase.OkResult(taxOffices.Select(x => new TaxOffice2DiplayDto
        {
            Id = x.Id,
            FullName = x.FullName,
            ShortName = x.ShortName,
            Code = x.Code,
            ProvinceId = x.ProvinceId,
            ProvinceName = x.Province?.Name
        }).ToList());
    }
    public async Task<ResponseBase> GetAllTaxOffice2(PageRequest req)
    {

        var query = taxRepo2.Find(x => !x.Deleted, sortBy: req.SortBy, order: req.OrderBy);

        if (!string.IsNullOrEmpty(req.Keyword))
        {
            query = query.Where(x => x.FullName.Contains(req.Keyword)
                                    || x.ShortName!.Contains(req.Keyword)
                                    || x.Code.Contains(req.Keyword));
        }
        var taxOffices = await query.AsNoTracking()
                                    .ToPagedListAsync(req.Page, req.Size);

        return ResponseBase.OkResult(taxOffices.MapPagedList(x => new TaxOffice2DiplayDto
        {
            Id = x.Id,
            FullName = x.FullName,
            ShortName = x.ShortName,
            Code = x.Code,
            ProvinceId = x.ProvinceId,
            ProvinceName = x.Province?.Name
        }));
    }

    public async Task<ResponseBase> GetTaxOffice2ById(int id)
    {
        var found = await taxRepo2.Find(x => x.Id == id && !x.Deleted)
                                  .FirstOrDefaultAsync();
        if (found is null)
        {
            return ResponseBase.Error404("Not found");
        }
        return ResponseBase.OkResult(new TaxOffice2DiplayDto
        {
            Id = found.Id,
            FullName = found.FullName,
            ShortName = found.ShortName,
            Code = found.Code,
            ProvinceId = found.ProvinceId,
            ProvinceName = found.Province?.Name
        });
    }

    public async Task<ResponseBase> UpdateTaxOffice2(int id, TaxOffice2CreateDto input)
    {
        try
        {
            var found = await taxRepo2.FindByIdAsync(id) ?? throw new NotFoundException("Invalid tax office's ID");
            if (input.ProvinceId is not null && !await provinceRepo.ExistAsync(x => x.Id == input.ProvinceId && !x.Deleted))
                return ResponseBase.Error400("Province could not be found");

            found.FullName= input.FullName;
            found.ShortName = input.ShortName;
            found.Code = input.Code;
            found.ProvinceId = input.ProvinceId;

            await taxRepo2.UpdateAsync(found);
            return ResponseBase.Ok();
        }
        catch (NotFoundException e)
        {
            return ResponseBase.Error404(e.Message);
        }
        catch (Exception e)
        {
            return ResponseBase.Error500(e.Message);
        }
    }
    #endregion

}