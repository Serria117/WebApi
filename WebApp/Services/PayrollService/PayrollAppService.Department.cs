using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

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

    public async Task<ResponseEntity> GetDepartmentsAsync(RequestParam requestParam)
    {
        var orgId = WorkingOrg.ToGuid();
        if (orgId == Guid.Empty)
            throw new InvalidOperationException("Working organization is not set.");

        var query = DepartmentRepository.Find(d => d.OrganizationId == orgId && !d.Deleted);
        if (requestParam.Keyword != null)
        {
            query = query.Where(d => d.Name.Contains(requestParam.Keyword));
        }

        var departments = await query.Select(d => new
                                     {
                                         d.Id, d.Name, d.Code
                                     })
                                     .ToListAsync();
        return ResponseEntity.OkResult(departments);
    }

    public async Task<ResponseEntity> GetDepartmentByIdAsync(string id)
    {
        var orgId = WorkingOrg.ToGuid();
        if (orgId == Guid.Empty)
            throw new InvalidOperationException("Working organization is not set.");
        var department = await DepartmentRepository.Find(d => d.Id == id && d.OrganizationId == orgId)
                                                   .FirstOrDefaultAsync();
        return department == null
            ? ResponseEntity.Error404("Department not found.")
            : ResponseEntity.OkResult(department);
    }

    public async Task<bool> IsDepartmentExistAsync(string name)
        => await DepartmentRepository.ExistAsync(d => d.OrganizationId == WorkingOrg.ToGuid()
                                                      && !d.Deleted
                                                      && d.Name == name);
}