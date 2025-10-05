using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task<ResponseBase> CreateAllowanceType(AllowanceTypeCreate dto)
    {
        var newAllowanceType = dto.ToEntity(WorkingOrg.ToGuid() == Guid.Empty ? null : WorkingOrg.ToGuid());
        await AllowanceTypesRepository.CreateAsync(newAllowanceType);
        return ResponseBase.OkResult(newAllowanceType);
    }

    public async Task<ResponseBase> GetAllowanceTypes()
    {
        var result = await AllowanceTypesRepository
                           .Find(x => !x.Deleted
                                      && (x.OrganizationId == null || x.OrganizationId == WorkingOrg.ToGuid()))
                           .OrderBy(x => x.Id)
                           .ToListAsync();
        return ResponseBase.OkResult(result.Select(a => new AllowanceTypeDisplay
        {
            Id = a.Id,
            Name = a.Name,
            Code = a.Code,
            IsTaxable = a.IsTaxable,
            IsInsurance = a.IsInsurance,
            OrganizationId = a.OrganizationId,
            MaxAmount = a.MaxAmount,
            DefaultAmount = a.DefaultAmount,
            Unit = a.Unit
        }));
    }

    private async Task SetDefaultAllowanceForEmployee(Employee employee)
    {
        var allowanceTypes = await AllowanceTypesRepository
                                   .Find(a => !a.Deleted && (a.OrganizationId == WorkingOrg.ToGuid() 
                                                             || a.OrganizationId == null))
                                   .ToListAsync();
        
    }
}