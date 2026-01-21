using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Pqc.Crypto.Utilities;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities.Tax;
using WebApp.Payloads;
using WebApp.Services.Mappers;
using WebApp.Services.TaxRegulationServices.Dto;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.TaxRegulationServices;

public interface ISocialSecurityService
{
    Task<ResponseEntity> CreateSocialSecurityRegulation(SocialSecurityDto input);
    Task<ResponseEntity> FindSocialSecurityRegulations(string? keyword);
    Task<ResponseEntity> GetSocialSecurityRegulationById(Guid id);
    Task<ResponseEntity> SoftDelete(Guid id);
    Task<ResponseEntity> UpdateSocialSecurityRegulation(Guid id, SocialSecurityDto input);
}

public class SocialSecurityService(IUserManager userManager,
                                   AppDbContext dbContext) : BaseAppService(userManager), ISocialSecurityService
{
    public async Task<ResponseEntity> CreateSocialSecurityRegulation(SocialSecurityDto input)
    {
        var newRegulation = new SocialSecurityRegulation
        {
            Name = input.Name,
            EffectiveDate = input.EffectiveDate.ToLocalTime(),
            EndDate = input.EndDate.HasValue ? input.EndDate.Value.ToLocalTime() : null,
            EmployeeRate = input.EmployeeRate,
            EmployerRate = input.EmployerRate,
            Order = input.Order
        };
        await dbContext.SocialSecurityRegulations.AddAsync(newRegulation);
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> FindSocialSecurityRegulations(string? keyword)
    {
        var regulations = await dbContext.SocialSecurityRegulations
                                         .Where(x => !x.Deleted)
                                         .WhereIf(!string.IsNullOrEmpty(keyword),
                                                  x => x.Name.Contains(keyword!))
                                         .ToListAsync();
        return ResponseEntity.OkResult(regulations.Select(x => x.ToDisplayDto()));
    }

    public async Task<ResponseEntity> GetSocialSecurityRegulationById(Guid id)
    {
        var found = await dbContext.SocialSecurityRegulations.FindAsync(id);
        if (found != null)
        {
            return ResponseEntity.OkResult(found.ToDisplayDto());
        }
        return ResponseEntity.Error404("Social Security regulation not found");
    }

    public async Task<ResponseEntity> UpdateSocialSecurityRegulation(Guid id, SocialSecurityDto input)
    {
        var existing = await dbContext.SocialSecurityRegulations.FindAsync(id);
        if (existing == null)
        {
            return ResponseEntity.Error404("Social Security regulation not found");
        }
        existing.Name = input.Name;
        existing.EffectiveDate = input.EffectiveDate;
        existing.EndDate = input.EndDate;
        existing.EmployeeRate = input.EmployeeRate;
        existing.EmployerRate = input.EmployerRate;
        existing.Order = input.Order;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }

    public async Task<ResponseEntity> SoftDelete(Guid id)
    {
        var existing = await dbContext.SocialSecurityRegulations.FindAsync(id);
        if (existing == null)
        {
            return ResponseEntity.Error404("Social Security regulation not found");
        }
        existing.Deleted = true;
        await dbContext.SaveChangesAsync();
        return ResponseEntity.Ok();
    }
}
