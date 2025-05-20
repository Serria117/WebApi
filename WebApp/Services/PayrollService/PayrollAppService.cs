using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Salary;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.UserService;

namespace WebApp.Services.PayrollService;

public interface IPayrollAppService
{
    Task<AppResponse> CreateEmployeeAsync(EmployeeCreateDto input);
    Task<AppResponse> CreateManyEmployeesAsync(CreateManyEmployeeRequest request);
    Task<List<PayrollRecord>> GetPayrollRecordsAsync();
}

public class PayrollAppService(IUserManager userManager, 
                               IUnitOfWork transactionManager,
                               ILogger<PayrollAppService> log) : AppServiceBase(userManager), IPayrollAppService
{
    private IAppRepository<PayrollRecord, long> PayrollRepository 
        => transactionManager.GetRepository<PayrollRecord, long>();
    private IAppRepository<Employee, Guid> EmployeeRepository 
        => transactionManager.GetRepository<Employee, Guid>();
    private IAppRepository<Organization, Guid> OrganizationRepository 
        => transactionManager.GetRepository<Organization, Guid>();

    public async Task<AppResponse> CreateEmployeeAsync(EmployeeCreateDto input)
    {
        try
        {
            var org = await OrganizationRepository.Find(x => x.Id == WorkingOrg.ToGuid())
                                                  .FirstOrDefaultAsync();
            if (org == null) throw new Exception("Organization not found");
            await transactionManager.BeginTransactionAsync();
            var employee = new Employee
            {
                FullName = input.FullName,
                Pid = input.Pid,
                TaxId = input.TaxId,
                HireDate = input.HireDate,
                TerminationDate = input.TerminationDate,
                Organization = org
            };
            var result = await EmployeeRepository.CreateAsync(employee, inTransaction: true);
            await transactionManager.CommitAsync();
            return AppResponse.SuccessResponse(new
            {
                result.FullName, result.Pid, result.TaxId, result.HireDate, result.TerminationDate
            });
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogError("Error in CreateEmployeeAsync: {Message}", e.Message);
            log.LogError(e, "Error in CreateEmployeeAsync");
            throw;
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> CreateManyEmployeesAsync(CreateManyEmployeeRequest req)
    {
        try
        {
            if(req.Employees.Count == 0) throw new EmptyResultException("Employee list is empty");
            var org = await OrganizationRepository.Find(x => x.Id == WorkingOrg.ToGuid())
                                                  .FirstOrDefaultAsync();
            if (org == null) throw new NotFoundException("Organization not found");
            await transactionManager.BeginTransactionAsync();
            var employees = req.Employees.Select(e => new Employee
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
            log.LogError("Error in CreateManyEmployeesAsync: {Message}", e.Message);
            log.LogError(e, "Error in CreateManyEmployeesAsync");
            throw;
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<List<PayrollRecord>> GetPayrollRecordsAsync()
    {
        try
        {
            await transactionManager.BeginTransactionAsync();
            var result = await PayrollRepository.Find(x => !x.Deleted).ToListAsync();
            //Some business logic can be added here
            await transactionManager.CommitAsync();
            return result;
        }
        catch (Exception e)
        {
            log.LogError("Error in GetPayrollRecordsAsync: {Message}. " +
                         "Operation will be safely rolled back.", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
            await transactionManager.RollbackAsync();
            throw;
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }
}