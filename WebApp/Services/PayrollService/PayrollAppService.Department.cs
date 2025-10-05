using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task CreateDepartmentAsync(DepartmentCreate dto)
    {
        var orgId = WorkingOrg.ToGuid();
        if (orgId == Guid.Empty)
            throw new InvalidOperationException("Working organization is not set.");
        var newDep = new Department
        {
            Id = Ulid.NewUlid().ToString(),
            Name = dto.Name,
            Code = dto.Code,
            OrganizationId = orgId
        };
        await DepartmentRepository.CreateAsync(newDep);
    }

    public async Task<ResponseBase> GetDepartmentsAsync()
    {
        var orgId = WorkingOrg.ToGuid();
        if (orgId == Guid.Empty)
            throw new InvalidOperationException("Working organization is not set.");

        var departments = await DepartmentRepository.Find(d => d.OrganizationId == orgId).ToListAsync();
        return ResponseBase.OkResult(departments);
    }

    public async Task<ResponseBase> GetDepartmentByIdAsync(string id)
    {
        var orgId = WorkingOrg.ToGuid();
        if (orgId == Guid.Empty)
            throw new InvalidOperationException("Working organization is not set.");
        var department = await DepartmentRepository.Find(d => d.Id == id && d.OrganizationId == orgId).FirstOrDefaultAsync();
        if (department == null)
            return ResponseBase.Error404("Department not found.");

        return ResponseBase.OkResult(department);
    }
}
