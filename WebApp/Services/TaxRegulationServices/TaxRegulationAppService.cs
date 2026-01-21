using Microsoft.EntityFrameworkCore;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities.Tax;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.Mappers;
using WebApp.Services.TaxRegulationServices.Dto;
using WebApp.Services.UserService;
using WebApp.Utils;

namespace WebApp.Services.TaxRegulationServices;

public interface ITaxRegulationAppService
{
    Task<decimal> CalculateMonthlyPIT(decimal taxableIncome, Guid regulationId);
    Task CreatePITRegulation(PersonalIncomeRegulationDto input);
    Task<ResponseEntity> FindPITRegulations(string? keyword);
    Task<ResponseEntity> GetPITRegulationById(Guid id);
}

public class TaxRegulationAppService(IUserManager userManager,
                                     AppDbContext dbContext)
    : BaseAppService(userManager), ITaxRegulationAppService
{
    public async Task<ResponseEntity> GetPITRegulationById(Guid id)
    {
        var found = await dbContext.PITRegulations.FindAsync(id);
        if (found != null)
        {
            return ResponseEntity.OkResult(new PersonalIncomeRegulationDisplayDto
            {
                Id = found.Id,
                Name = found.Name,
                MonthBrackets = found.MonthBrackets,
                YearBrackets = found.YearBrackets,
                EffectiveDate = found.EffectiveDate,
                EndDate = found.EndDate
            });
        }
        return ResponseEntity.Error404("Personal Income Tax regulation not found");
    }

    public async Task<ResponseEntity> FindPITRegulations(string? keyword)
    {
        var regulations = await dbContext.PITRegulations.Where(x => !x.Deleted)
                                         .WhereIf(!string.IsNullOrEmpty(keyword),
                                                  x => x.Name.Contains(keyword!))
                                         .OrderBy(x => x.Order)
                                         .ToListAsync();

        return ResponseEntity.OkResult(regulations);
    }

    public async Task CreatePITRegulation(PersonalIncomeRegulationDto input)
    {
        input.ValidBracketStep();
        var newRegulation = new PITRegulation
        {
            Name = input.Name,
            MonthBrackets = input.MonthBrackets,
            YearBrackets = input.YearBrackets,
            EffectiveDate = input.EffectiveDate.ToLocalTime(),
            EndDate = input.EndDate?.ToLocalTime(),
            Deductions = input.Deductions,
            Description = input.Description,
            Order = input.Order
        };
        await dbContext.PITRegulations.AddAsync(newRegulation);
        await dbContext.SaveChangesAsync();
    }

    public async Task<decimal> CalculateMonthlyPIT(decimal taxableIncome, Guid regulationId)
    {
        var regulation = await dbContext.PITRegulations
                                        .Where(x => x.Id == regulationId && !x.Deleted)
                                        .FirstOrDefaultAsync();
        if (regulation == null) { return 0m; }

        var bracket = regulation.MonthBrackets?
                                .Where(b => b.Limit.HasValue && b.Limit.Value >= taxableIncome)
                                .OrderBy(b => b.Limit!.Value)
                                .FirstOrDefault();
        if(bracket == null) return 0m;
        var tax = (taxableIncome * bracket.Rate) - bracket.ProgressiveAmount;
        return Math.Max(0, tax);
    }
}
