using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Salary;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    #region EMPLOYEE

    public async Task<AppResponse> CreateEmployeeAsync(EmployeeCreateDto input)
    {
        try
        {
            var org = await OrganizationRepository.Find(x => x.Id == WorkingOrg.ToGuid())
                                                  .FirstOrDefaultAsync();
            if (org == null) throw new Exception("Organization not found");
            await transactionManager.BeginAsync();
            var employee = input.ToEntity(org);

            var result = await EmployeeRepository.CreateAsync(employee, inTransaction: true);
            // Create dependents if any
            if (input.Dependents.Count > 0)
            {
                var dependents = input.Dependents.Select(d => d.ToEntity(result)).ToList();
                await DependentRepository.CreateManyAsync(dependents, inTransaction: true);
            }

            await transactionManager.CommitAsync();
            log.LogInfoFormatted();
            return AppResponse.Ok($"Employee {result.FullName} - {result.TaxId} created successfully");
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> CreateManyEmployeesAsync(ICollection<EmployeeCreateDto> input)
    {
        try
        {
            if (input.Count == 0) throw new EmptyResultException("Employee list is empty");
            var orgId = WorkingOrg.ToGuid();
            if (orgId == Guid.Empty)
                throw new InvalidOperationException("Organization ID is invalid");
            var org = OrganizationRepository.Attach(orgId);
            await transactionManager.BeginAsync();
            var employees = input.Select(e => new Employee
            {
                FullName = e.FullName,
                Pid = e.Pid,
                TaxId = e.TaxId,
                HireDate = e.HireDate,
                TerminationDate = e.TerminationDate,
                Organization = org
            }).ToList();
            await EmployeeRepository.CreateManyAsync(employees, inTransaction: true);
            await transactionManager.CommitAsync();
            return AppResponse.Ok();
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogErrorFormatted(exception: e);
            throw;
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> GetEmployeesAsync(PageRequest request)
    {
        try
        {
            var result = await EmployeeRepository
                               .Find(x => !x.Deleted
                                          && x.OrganizationId == WorkingOrg.ToGuid())
                               .Include(x => x.Dependents)
                               .OrderBy(x => x.CreateAt)
                               .ToPagedListAsync(request.Page, request.Size);
            return result.Count == 0
                ? AppResponse.Ok("Current Organization has no employees")
                : AppResponse.OkResult(result.MapPagedList(x => x.ToDisplayDto()));
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            var result = await EmployeeRepository
                               .Find(x => x.Id == id
                                          && !x.Deleted
                                          && x.OrganizationId == WorkingOrg.ToGuid())
                               .Include(x => x.Dependents)
                               .FirstOrDefaultAsync();
            return result == null
                ? AppResponse.Error404("Employee not found")
                : AppResponse.OkResult(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> SoftDeleteEmployeeAsync(Guid id)
    {
        try
        {
            var employee = await EmployeeRepository.Find(x => x.Id == id
                                                              && !x.Deleted
                                                              && x.OrganizationId == WorkingOrg.ToGuid())
                                                   .FirstOrDefaultAsync();
            if (employee == null)
                return AppResponse.Error404("Employee not found");

            employee.Deleted = true;
            await EmployeeRepository.UpdateAsync(employee);
            return AppResponse.Ok("Employee soft-deleted successfully");
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> SoftDeleteManyEmployeesAsync(Guid[] ids)
    {
        try
        {
            var employees = await EmployeeRepository
                                  .Find(x => ids.Contains(x.Id)
                                             && !x.Deleted
                                             && x.OrganizationId == WorkingOrg.ToGuid())
                                  .Select(x => x.Id)
                                  .ToListAsync();
            if (employees.Count == 0) return AppResponse.Error("No employees found");

            await EmployeeRepository.SoftDeleteManyAsync(employees.ToArray());
            return AppResponse.Ok("Employees soft-deleted successfully");
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> CreateDependentsAsync(Guid employeeId,
                                                         ICollection<DependentCreateDto> input)
    {
        try
        {
            var employee = await EmployeeRepository
                                 .Find(x => x.Id == employeeId
                                            && !x.Deleted
                                            && x.OrganizationId == WorkingOrg.ToGuid())
                                 .FirstOrDefaultAsync();
            if (employee == null)
                return AppResponse.Error("Employee not found");

            var dependents = input.Select(d => d.ToEntity(employee)).ToList();
            await DependentRepository.CreateManyAsync(dependents);
            return AppResponse.Ok("Dependents created successfully");
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    #endregion
}