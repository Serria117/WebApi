using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.IsisMtt.X509;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    public async Task<ResponseBase> CreateEmployeeAsync(EmployeeCreateDto dto)
    {
        try
        {
            await transaction.BeginAsync();
            var org = await OrganizationRepository.Find(o => o.Id == WorkingOrg.ToGuid())
                                                  .FirstOrDefaultAsync();
            if (org is null)
                return ResponseBase.Error404("Organization not found");

            //check if employee's taxId existing in the organization:
            var existEmployeeTaxId = await IsEmployeeDuplicatedAsync(dto.TaxId, dto.PersonalId);
            if (existEmployeeTaxId)
                return ResponseBase.Error($"Employee with taxId [{dto.TaxId}] already exists");

            var employee = dto.ToEntity(org);
            if (dto.Dependents.Count > 0)
            {
                employee.Dependents = [.. dto.Dependents.Select(d => d.ToEntity())];
            }

            /*if (dto.Allowances.Count > 0)
            {
                employee.Allowances = [..dto.Allowances.Select(a => a.ToEntity())];
            }*/

            var savedEmployee = await EmployeeRepository.CreateAsync(employee, inTransaction: true);

            await CreateSalaryAsync(savedEmployee, dto.SalaryValue, dto.InsuranceSalaryValue, dto.SalaryStartDate,
                                    dto.SalaryEndDate);

            await transaction.CommitAsync();
            return ResponseBase.OkResult(savedEmployee.ToDisplayDto());
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            await transaction.RollbackAsync();
            return ResponseBase.Error(ResponseMessage.GenericError);
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<ResponseBase> UpdateEmployeeAsync(long id, EmployeeUpdate dto)
    {
        try
        {
            await transaction.BeginAsync();
            var employee = await EmployeeRepository.Find(e => e.Id == id && !e.Deleted)
                                                   .Include(e => e.Dependents)
                                                   .FirstOrDefaultAsync();
            if (employee is null) return ResponseBase.Error404("Employee not found");

            //Update entity then save change to database:
            employee.Name = dto.Name;
            employee.PersonalId = dto.PersonalId;
            employee.TaxId = dto.TaxId;
            employee.EndDate = dto.EndDate;
            employee.JoinDate = dto.JoinDate;

            var existingDependents = employee.Dependents.ToDictionary(x => x.Id);
            var updateDependents = dto.Dependents;

            List<Dependents> dependentToUpdate = [];

            foreach (var dependent in updateDependents)
            {
                //if item has no Id, means it is a new one then create new entity
                if (dependent.Id == null)
                {
                    var newDependent = new Dependents
                    {
                        Name = dependent.Name,
                        TaxId = dependent.TaxId,
                        PersonalId = dependent.PersonalId,
                        DateOfBirth = dependent.DateOfBirth,
                        EffectiveDate = dependent.EffectiveDate,
                        EndDate = dependent.EndDate,
                        Relationship = dependent.Relationship,
                    };
                    dependentToUpdate.Add(newDependent);
                    continue;
                }

                if (!existingDependents.TryGetValue(dependent.Id.Value, out var editingDependent))
                    continue;

                editingDependent.Name = dependent.Name;
                editingDependent.PersonalId = dependent.PersonalId;
                editingDependent.TaxId = dependent.TaxId;
                editingDependent.DateOfBirth = dependent.DateOfBirth;
                editingDependent.Relationship = dependent.Relationship;
                editingDependent.EffectiveDate = dependent.EffectiveDate;
                editingDependent.EndDate = dependent.EndDate;

                dependentToUpdate.Add(editingDependent);
                existingDependents.Remove(dependent.Id.Value);
            }

            foreach (var toDelete in existingDependents.Values)
            {
                employee.Dependents.Remove(toDelete);
            }

            employee.Dependents = dependentToUpdate;

            await EmployeeRepository.UpdateAsync(employee);
            await transaction.CommitAsync();
            return ResponseBase.OkResult(employee.ToDisplayDto());
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            await transaction.RollbackAsync();
            return ResponseBase.Error(ResponseMessage.GenericError);
        }
        finally
        {
            await transaction.DisposeAsync();
        }
    }

    public async Task<ResponseBase> GetEmployeesAsync(EmployeeQuery query)
    {
        var emp = await EmployeeRepository
                        .Find(e => e.OrganizationId == WorkingOrg.ToGuid()
                                   && !e.Deleted
                                   && (query.Keyword == null || (e.Name.Contains(query.Keyword) ||
                                                                 e.TaxId == query.Keyword ||
                                                                 e.PersonalId == query.Keyword)
                                       && (query.Year == null || (e.JoinDate.Year <= query.Year &&
                                                                   (e.EndDate == null || e.EndDate.Value.Year >= query.Year))
                                           )
                                   )
                        )
                        /*.Include(e => e.Salaries)
                        .Include(e => e.Dependents)
                        .Include(e => e.Allowances)*/
                        .AsNoTracking()
                        .ToListAsync();
        return ResponseBase.OkResult(emp);
    }

    public async Task<ResponseBase> GetEmployeesInPeriod(long periodId)
    {
        var period = await GetPayrollPeriod(periodId);
        if (period is null) return ResponseBase.Error404("PayrollPeriod not found.");
        var employees = await EmployeeRepository
                              .Find(e => e.OrganizationId == WorkingOrg.ToGuid()
                                         && !e.Deleted
                                         && e.JoinDate <= period.StartDate
                                         && (e.EndDate >= period.EndDate || e.EndDate == null))
                              .Include(e => e.Dependents)
                              .Include(e => e.Salaries.Where(s => s.EffectiveDate <= period.StartDate
                                                                  && (s.EndDate >= period.EndDate || s.EndDate == null)))

                              .ToListAsync();
        return ResponseBase.OkResult(employees);
    }

    public async Task<ResponseBase> GetEmployeeById(long id)
    {
        var employee = await EmployeeRepository.Find(x => x.Id == id)
                                               .Include(x => x.Dependents)
                                               .Include(x => x.Salaries)
                                               .Include(x => x.Allowances)
                                               .FirstOrDefaultAsync();
        return employee is not null
            ? ResponseBase.OkResult(employee.ToDisplayDto())
            : ResponseBase.Error404("Employee not found");
    }

    public async Task<ResponseBase> DeleteEmployeesAsync(List<long> idList)
    {
        var result = await EmployeeRepository.SoftDeleteManyAsync([.. idList]);
        return result
            ? ResponseBase.OkResult(result)
            : ResponseBase.Error400("Failed to delete employee");
    }

    public async Task<ResponseBase> AddSalaryToEmployeeAsync(SalaryCreate dto)
    {
        var emp = await EmployeeRepository.Find(e => e.Id == dto.EmployeeId).FirstOrDefaultAsync();
        if (emp is null) return ResponseBase.Error404("Employee not found");
        return ResponseBase.OkResult(await CreateSalaryAsync(emp,
                                                            dto.SalaryValue, dto.InsuranceSalaryValue,
                                                            dto.EffectiveDate, dto.EndDate));
    }

    public async Task<ResponseBase> EditDependentsInEmployeeAsync(long employeeId,
                                                                 ICollection<DependentCreateDto> dtoList)
    {
        var emp = await EmployeeRepository.Find(e => e.Id == employeeId
                                                     && e.Deleted == false
                                                     && e.OrganizationId == WorkingOrg.ToGuid())
                                          .Include(e => e.Dependents.Where(d => d.Deleted == false))
                                          .FirstOrDefaultAsync();
        if (emp is null) return ResponseBase.Error404("Employee not found");

        var dependentList = dtoList.Select(item => new Dependents
        {
            Name = item.Name,
            DateOfBirth = item.DateOfBirth,
            EffectiveDate = item.EffectiveDate,
            EndDate = item.EndDate,
            Employee = emp,
            TaxId = item.TaxId,
            PersonalId = item.PersonalId,
            Relationship = item.Relationship,
        }).Where(d => emp.Dependents.All(e => e.Name != d.Name)
                      && emp.Dependents.All(e => e.TaxId == d.TaxId))
                                   .ToList();

        await DependentsRepository.CreateManyAsync(dependentList);
        return ResponseBase.OkResult(dependentList);
    }

    

    private async Task<Salary> CreateSalaryAsync(Employee employee,
                                                 decimal salary, decimal? insurance,
                                                 DateTime effectiveDate, DateTime? endDate)
    {
        var newSalary = new Salary
        {
            SalaryValue = salary,
            InsuranceSalaryValue = insurance,
            EffectiveDate = effectiveDate,
            EndDate = endDate,
            Employee = employee
        };
        var addedSalary = await SalaryRepository.CreateAsync(newSalary, inTransaction: true);
        return addedSalary;
    }

    private async Task<bool> IsEmployeeDuplicatedAsync(string? taxId, string? pId)
    {
        if (taxId is null && pId is null) return false;
        return await EmployeeRepository.ExistAsync(e => e.OrganizationId == WorkingOrg.ToGuid()
                                                        && (e.TaxId == taxId || e.PersonalId == pId));
    }
}