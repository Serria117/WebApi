using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Salary;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.UserService;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.PayrollService;

public interface IPayrollAppService
{
    /// <summary>
    /// Creates a new employee for the current organization.
    /// </summary>
    /// <param name="input">The data transfer object containing the employee creation details.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> indicating the result of the operation.
    /// </returns>
    Task<AppResponse> CreateEmployeeAsync(EmployeeCreateDto input);

    /// <summary>
    /// Creates multiple employees for the current organization.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>
    /// An <see cref="AppResponse"/> indicating the result of the operation.
    /// </returns>
    Task<AppResponse> CreateManyEmployeesAsync(ICollection<EmployeeCreateDto> input);

    /// <summary>
    /// Retrieves a paginated list of employees for the current organization.
    /// </summary>
    /// <param name="request">The pagination and filtering parameters.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> containing the paginated list of employees or an appropriate message if no employees are found.
    /// </returns>
    Task<AppResponse> GetEmployeesAsync(PageRequest request);

    /// <summary>
    /// Retrieves an employee by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to retrieve.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> containing the employee details if found, 
    /// or an error message if the employee does not exist.
    /// </returns>
    Task<AppResponse> GetEmployeeByIdAsync(Guid id);

    /// <summary>
    /// Soft deletes an employee by marking them as deleted.
    /// </summary>
    /// <param name="id">The unique identifier of the employee to be soft deleted.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> indicating the result of the operation.
    /// </returns>
    Task<AppResponse> SoftDeleteEmployeeAsync(Guid id);

    /// <summary>
    /// Soft deletes multiple employees by marking them as deleted.
    /// </summary>
    /// <param name="ids">An array of employee IDs to be soft deleted.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> indicating the result of the operation.
    /// </returns>
    Task<AppResponse> SoftDeleteManyEmployeesAsync(Guid[] ids);

    /// <summary>
    /// Retrieves a list of payroll records that are not marked as deleted.
    /// </summary>
    /// <remarks>
    /// This method fetches all payroll records from the database where the `Deleted` flag is set to false.
    /// It ensures that only active payroll records are returned.
    /// </remarks>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of <see cref="PayrollRecord"/> objects.
    /// </returns>
    Task<AppResponse> GetPayrollRecordsAsync(int periodId);

    /// <summary>
    /// Creates a new payroll period for the current organization.
    /// </summary>
    /// <param name="input">The payroll period creation data.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation.</returns>
    Task<AppResponse> CreatePayrollPeriodAsync(PayrollPeriodCreateDto input);

    Task<AppResponse> CreatePayrollComponentCategory(PayrollComponentCategoryCreateDto input);
    Task<AppResponse> CreatePayrollInputType(PayrollInputTypeCreateDto input);
}

public class PayrollAppService(IUserManager userManager,
                               IUnitOfWork transactionManager,
                               ILogger<PayrollAppService> log) : AppServiceBase(userManager), IPayrollAppService
{
    #region Repositories injection

    private IAppRepository<PayrollRecord, long> PayrollRepository
        => transactionManager.GetRepository<PayrollRecord, long>();

    private IAppRepository<Employee, Guid> EmployeeRepository
        => transactionManager.GetRepository<Employee, Guid>();

    private IAppRepository<Organization, Guid> OrganizationRepository
        => transactionManager.GetRepository<Organization, Guid>();

    private IAppRepository<TimeSheet, long> TimeSheetRepository
        => transactionManager.GetRepository<TimeSheet, long>();

    private IAppRepository<PayrollPeriod, int> PayrollPeriodRepository
        => transactionManager.GetRepository<PayrollPeriod, int>();

    private IAppRepository<PayrollItem, long> PayrollItemRepository
        => transactionManager.GetRepository<PayrollItem, long>();

    private IAppRepository<PayrollComponentCategory, int> PayrollComponentCategoryRepository
        => transactionManager.GetRepository<PayrollComponentCategory, int>();

    private IAppRepository<PayrollInputType, int> PayrollInputTypeRepository 
        => transactionManager.GetRepository<PayrollInputType, int>();
    
    private IAppRepository<PayrollInput, int> PayrollInputRepository
        => transactionManager.GetRepository<PayrollInput, int>();
    
    
    #endregion

    #region EMPLOYEE

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
            return AppResponse.SuccessResponse(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogError("Error in CreateEmployeeAsync: {Message}", e.Message);
            log.LogError(e, "Error in CreateEmployeeAsync");
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
            await transactionManager.BeginTransactionAsync();
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
            log.LogError("Error in CreateManyEmployeesAsync: {Message}", e.Message);
            log.LogError(e, "Error in CreateManyEmployeesAsync");
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
            var result = await EmployeeRepository.Find(x => !x.Deleted
                                                            && x.OrganizationId == WorkingOrg.ToGuid())
                                                 .ToPagedListAsync(request.Page, request.Size);
            return result.Count == 0
                ? AppResponse.Ok("Current Organization has no employees")
                : AppResponse.SuccessResponse(result.MapPagedList(x => x.ToDisplayDto()));
        }
        catch (Exception e)
        {
            log.LogError("Error in GetEmployeesAsync: {Message}. " +
                         "Operation will be safely rolled back.", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> GetEmployeeByIdAsync(Guid id)
    {
        try
        {
            var result = await EmployeeRepository.Find(x => x.Id == id
                                                            && !x.Deleted
                                                            && x.OrganizationId == WorkingOrg.ToGuid())
                                                 .FirstOrDefaultAsync();
            return result == null
                ? AppResponse.Error("Employee not found")
                : AppResponse.SuccessResponse(result);
        }
        catch (Exception e)
        {
            log.LogError("Error in GetEmployeeByIdAsync: {Message}. " +
                         "Operation will be safely rolled back.", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
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
                return AppResponse.Error("Employee not found");

            employee.Deleted = true;
            await EmployeeRepository.UpdateAsync(employee);
            return AppResponse.Ok("Employee soft-deleted successfully");
        }
        catch (Exception e)
        {
            log.LogError("Error in SoftDeleteEmployeeAsync: {Message}", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
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
            log.LogError("Error in SoftDeleteManyEmployeesAsync: {Message}", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
            return AppResponse.Error(e.Message);
        }
    }

    #endregion

    #region PAYROLL

    public async Task<AppResponse> CreatePayrollPeriodAsync(PayrollPeriodCreateDto input)
    {
        try
        {
            var org = await OrganizationRepository.Find(x => x.Id == WorkingOrg.ToGuid())
                                                  .FirstOrDefaultAsync();
            if (org == null) return AppResponse.Error("Organization not found");

            var period = new PayrollPeriod
            {
                Name = input.Name,
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Organization = org
            };

            await transactionManager.BeginTransactionAsync();
            var result = await PayrollPeriodRepository.CreateAsync(period);
            await transactionManager.CommitAsync();

            return AppResponse.SuccessResponse(result);
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogError("Error in CreatePayrollPeriodAsync: {Message}", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
            return AppResponse.Error(e.Message);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> GetPayrollRecordsAsync(int periodId)
    {
        try
        {
            await transactionManager.BeginTransactionAsync();
            var result = await PayrollRepository.Find(x => !x.Deleted && periodId == x.Id)
                                                .Include(x => x.Employee)
                                                .ToListAsync();
            //Some business logic can be added here
            await transactionManager.CommitAsync();
            return AppResponse.SuccessResponse(result);
        }
        catch (Exception e)
        {
            log.LogError("Error in GetPayrollRecordsAsync: {Message}. " +
                         "Operation will be safely rolled back.", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
            await transactionManager.RollbackAsync();
            return AppResponse.Error(e.Message);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    #endregion

    #region PAYROLL CONFIGURATION

    public async Task<AppResponse> CreatePayrollComponentCategory(PayrollComponentCategoryCreateDto input)
    {
        try
        {
            var category = new PayrollComponentCategory
            {
                Name = input.Name,
                Description = input.Description,
            };

            await transactionManager.BeginTransactionAsync();
            var result = await PayrollComponentCategoryRepository.CreateAsync(category, true);
            await transactionManager.CommitAsync();
            return AppResponse.SuccessResponse(result);
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogError("Error in CreatePayrollComponentCategory: {Message}", e.Message);
            log.LogError("Stack trace: {StackTrace}", e.StackTrace);
            return AppResponse.Error(e.Message);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> CreatePayrollInputType(PayrollInputTypeCreateDto input)
    {
        var item = input.ToEntity();
        var result = await PayrollInputTypeRepository.CreateAsync(item);
        return AppResponse.SuccessResponse(result);
    }
    
    

#endregion
}