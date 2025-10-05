using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spire.Xls;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.TaxProcedureService.dto;
using WebApp.Services.UserService;

namespace WebApp.Services.TaxProcedureService;

public interface ITaxProcedureAppService
{
    Task<ResponseBase> CreateTaxProcedure(TaxProcedureCreateDto dto);
    Task<ResponseBase> CreateManyTaxProcedures(ICollection<TaxProcedureCreateDto> dtos);
    Task<ResponseBase> DeleteManyTaxProcedures(int[] ids);
    Task<ResponseBase> DeleteTaxProcedure(int id);
    Task<ResponseBase> FindTaxProcedureById(int id);
    Task<ResponseBase> GetAllTaxProcedures(string? keyword);
    Task<ResponseBase> UpdateTaxProcedure(TaxProcedureUpdateDto dto);
}

public class TaxProcedureAppService(IAppRepository<TaxProcedure, int> taxProcedureRepository,
                                    IUserManager userManager)
    : BaseAppService(userManager), ITaxProcedureAppService
{
    public async Task<ResponseBase> CreateTaxProcedure(TaxProcedureCreateDto dto)
    {
        var taxProcedure = new TaxProcedure
        {
            Name = dto.Name,
            Code = dto.Code,
            Order = dto.Order,
            UnsignName = $"{dto.Code} - {dto.Name.UnSign()}",
            Description = dto.Description
        };
        await taxProcedureRepository.CreateAsync(taxProcedure);
        return ResponseBase.OkResult(taxProcedure);
    }

    public async Task<ResponseBase> CreateManyTaxProcedures(ICollection<TaxProcedureCreateDto> dtos)
    {
        var validationResults = new List<ValidationResult>();
        var validDtos = new List<TaxProcedureCreateDto>();
        var invalidDtos = new List<InvalidTaxProcedureDto>();

        foreach (var dto in dtos)
        {
            var validationContext = new ValidationContext(dto);
            if (Validator.TryValidateObject(dto, validationContext, validationResults, true))
            {
                validDtos.Add(dto);
            }
            invalidDtos.Add(new InvalidTaxProcedureDto
            {
                Error = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage)),
                TaxProcedure = dto
            });
        }

        var taxProcedures = validDtos.Select(dto => new TaxProcedure
        {
            Name = dto.Name,
            Code = dto.Code,
            Order = dto.Order,
            UnsignName = $"{dto.Code} - {dto.Name.UnSign()}",
            Description = dto.Description
        }).ToList();
        await taxProcedureRepository.CreateManyAsync(taxProcedures);

        return ResponseBase.OkResult(new
        {
            Success = validDtos,
            Errors = invalidDtos.Count > 0 ? invalidDtos : null,
        });
    }

    public async Task<ResponseBase> GetAllTaxProcedures(string? keyword)
    {
        keyword = keyword.RemoveSpace();
        var taxProcedures = await taxProcedureRepository.Find(x => !x.Deleted)
                                                        .Where(x => string.IsNullOrEmpty(keyword)
                                                                || (x.UnsignName.Contains(keyword)
                                                                || (x.Description != null && x.Description.Contains(keyword)))
                                                         )
                                                        .OrderBy(x => x.Order)
                                                        .Select(x => new TaxProcedureDisplayDto
                                                        {
                                                            Id = x.Id,
                                                            Name = x.Name,
                                                            Code = x.Code,
                                                            Description = x.Description,
                                                            Order = x.Order
                                                        }).ToListAsync();
        return ResponseBase.OkResult(taxProcedures);
    }

    public async Task<ResponseBase> FindTaxProcedureById(int id)
    {
        var found = await taxProcedureRepository.Find(x => x.Id == id && !x.Deleted)
                                                .Select(x => new TaxProcedureDisplayDto
                                                {
                                                    Id = x.Id,
                                                    Name = x.Name,
                                                    Code = x.Code,
                                                    Description = x.Description,
                                                    Order = x.Order
                                                }).FirstOrDefaultAsync();
        if (found is null) return ResponseBase.Error404("Tax procedure not found");
        return ResponseBase.OkResult(found);
    }

    public async Task<ResponseBase> UpdateTaxProcedure(TaxProcedureUpdateDto dto)
    {
        var found = await taxProcedureRepository.Find(x => x.Id == dto.Id && !x.Deleted).FirstOrDefaultAsync();
        if (found is null) return ResponseBase.Error404("Tax procedure not found");
        found.Name = dto.Name;
        found.Code = dto.Code;
        found.Order = dto.Order;
        found.Description = dto.Description;
        found.UnsignName = $"{dto.Code} - {dto.Name.UnSign()}";
        await taxProcedureRepository.UpdateAsync(found);
        return ResponseBase.OkResult(new TaxProcedureDisplayDto
        {
            Id = found.Id,
            Name = found.Name,
            Code = found.Code,
            Order = found.Order,
            Description = found.Description
        });
    }

    public async Task<ResponseBase> DeleteTaxProcedure(int id)
    {
        var deleteResult = await taxProcedureRepository.SoftDeleteAsync(id);
        return deleteResult ? ResponseBase.OkResult("Tax procedure deleted successfully")
                            : ResponseBase.Error404("Tax procedure not found");
    }

    public async Task<ResponseBase> DeleteManyTaxProcedures(int[] ids)
    {
        var deleteResult = await taxProcedureRepository.SoftDeleteManyAsync(ids);
        return deleteResult ? ResponseBase.OkResult("Tax procedure deleted successfully")
                            : ResponseBase.Error404("Tax procedure not found");
    }
}
