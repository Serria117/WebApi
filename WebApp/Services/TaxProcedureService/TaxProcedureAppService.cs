using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Spire.Xls;
using WebApp.Core.DomainEntities;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.TaxProcedureService.dto;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.TaxProcedureService;

public interface ITaxProcedureAppService
{
    Task<ResponseEntity> CreateTaxProcedure(TaxProcedureCreateDto dto);
    Task<ResponseEntity> CreateManyTaxProcedures(ICollection<TaxProcedureCreateDto> dtos);
    Task<ResponseEntity> DeleteManyTaxProcedures(int[] ids);
    Task<ResponseEntity> DeleteTaxProcedure(int id);
    Task<ResponseEntity> FindTaxProcedureById(int id);
    Task<ResponseEntity> GetAllTaxProcedures(string? keyword);
    Task<ResponseEntity> UpdateTaxProcedure(TaxProcedureUpdateDto dto);
}

public class TaxProcedureAppService(IAppRepository<TaxProcedure, int> taxProcedureRepository,
                                    IUserManager userManager)
    : BaseAppService(userManager), ITaxProcedureAppService
{
    public async Task<ResponseEntity> CreateTaxProcedure(TaxProcedureCreateDto dto)
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
        return ResponseEntity.OkResult(taxProcedure);
    }

    public async Task<ResponseEntity> CreateManyTaxProcedures(ICollection<TaxProcedureCreateDto> dtos)
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

        return ResponseEntity.OkResult(new
        {
            Success = validDtos,
            Errors = invalidDtos.Count > 0 ? invalidDtos : null,
        });
    }

    public async Task<ResponseEntity> GetAllTaxProcedures(string? keyword)
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
        return ResponseEntity.OkResult(taxProcedures);
    }

    public async Task<ResponseEntity> FindTaxProcedureById(int id)
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
        if (found is null) return ResponseEntity.Error404("Tax procedure not found");
        return ResponseEntity.OkResult(found);
    }

    public async Task<ResponseEntity> UpdateTaxProcedure(TaxProcedureUpdateDto dto)
    {
        var found = await taxProcedureRepository.Find(x => x.Id == dto.Id && !x.Deleted).FirstOrDefaultAsync();
        if (found is null) return ResponseEntity.Error404("Tax procedure not found");
        found.Name = dto.Name;
        found.Code = dto.Code;
        found.Order = dto.Order;
        found.Description = dto.Description;
        found.UnsignName = $"{dto.Code} - {dto.Name.UnSign()}";
        await taxProcedureRepository.UpdateAsync(found);
        return ResponseEntity.OkResult(new TaxProcedureDisplayDto
        {
            Id = found.Id,
            Name = found.Name,
            Code = found.Code,
            Order = found.Order,
            Description = found.Description
        });
    }

    public async Task<ResponseEntity> DeleteTaxProcedure(int id)
    {
        var deleteResult = await taxProcedureRepository.SoftDeleteAsync(id);
        return deleteResult ? ResponseEntity.OkResult("Tax procedure deleted successfully")
                            : ResponseEntity.Error404("Tax procedure not found");
    }

    public async Task<ResponseEntity> DeleteManyTaxProcedures(int[] ids)
    {
        var deleteResult = await taxProcedureRepository.SoftDeleteManyAsync(ids);
        return deleteResult ? ResponseEntity.OkResult("Tax procedure deleted successfully")
                            : ResponseEntity.Error404("Tax procedure not found");
    }
}
