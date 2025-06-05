using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Salary;
using WebApp.Enums.Payroll;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Payloads.Payroll;
using WebApp.Repositories;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.UserService;
using WebApp.Utils;
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
    Task<AppResponse> GetPayrollRecordsInPeriodAsync(int periodId);

    /// <summary>
    /// Creates a new payroll period for the current organization.
    /// </summary>
    /// <param name="input">The payroll period creation data.</param>
    /// <returns>An <see cref="AppResponse"/> indicating the result of the operation.</returns>
    Task<AppResponse> CreatePayrollPeriodAsync(PayrollPeriodCreateDto input);

    Task<AppResponse> CreatePayrollComponentCategory(PayrollComponentCategoryCreateDto input);
    Task<AppResponse> CreatePayrollInputType(PayrollInputTypeCreateDto input);
    Task<AppResponse> GetPayrollComponentCategories();
    Task<AppResponse> CreatePayrollComponentType(PayrollComponentTypeCreateDto input);
    Task<AppResponse> GetPayrollComponentTypes();
    Task<AppResponse> GetPayrollComponentTypeById(int id);
    Task<AppResponse> GetPayrollComponentCategoryById(int id);
    Task<AppResponse> GetPayrollPeriods(PayrollPeriodRequest request);
    Task<AppResponse> GetPayrollPeriodById(int id);
    Task<AppResponse> CreatePayrollRecordsAsync(PayrollRecordCreateDto input);
    Task<AppResponse> GetPayrollRecordByIdAsync(long id);

    /// <summary>
    /// Creates dependents for a specified employee.
    /// </summary>
    /// <param name="employeeId">The unique identifier of the employee to whom the dependents will be added.</param>
    /// <param name="input">A collection of <see cref="DependentCreateDto"/> objects representing the dependents to be created.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> indicating the result of the operation.
    /// </returns>
    Task<AppResponse> CreateDependentsAsync(Guid employeeId,
                                            ICollection<DependentCreateDto> input);

    /// <summary>
    /// Initializes payroll periods for all months of a given year.
    /// </summary>
    /// <param name="request">The <see cref="InitPayrollPeriodRequest"/> wrapper which contains value of year, net work days and weekend type. </param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<AppResponse> InitPayrollByYearAsync(InitPayrollPeriodRequest request);

    /// <summary>
    /// Updates the timesheets for the specified employees.
    /// </summary>
    /// <param name="dto">A list of <see cref="TimesheetUpdateDto"/> objects containing the timesheet update details.</param>
    /// <returns>
    /// An <see cref="AppResponse"/> indicating the result of the operation.
    /// </returns>
    Task<AppResponse> UpdateTimesheetsAsync(List<TimesheetUpdateDto> dto);
}

public partial class PayrollAppService(IUserManager userManager,
                                       IUnitOfWork transactionManager,
                                       ILogger<PayrollAppService> log)
    : BaseAppService(userManager), IPayrollAppService
{
    #region Repositories injection

    private IAppRepository<PayrollRecord, long> PayrollRecordRepository
        => transactionManager.GetRepository<PayrollRecord, long>();

    private IAppRepository<Employee, Guid> EmployeeRepository
        => transactionManager.GetRepository<Employee, Guid>();

    private IAppRepository<Organization, Guid> OrganizationRepository
        => transactionManager.GetRepository<Organization, Guid>();

    private IAppRepository<Timesheet, long> TimesheetRepository
        => transactionManager.GetRepository<Timesheet, long>();

    private IAppRepository<PayrollPeriod, int> PayrollPeriodRepository
        => transactionManager.GetRepository<PayrollPeriod, int>();

    private IAppRepository<PayrollItem, long> PayrollItemRepository
        => transactionManager.GetRepository<PayrollItem, long>();

    private IAppRepository<PayrollComponentCategory, int> PayrollComponentCategoryRepository
        => transactionManager.GetRepository<PayrollComponentCategory, int>();

    private IAppRepository<PayrollComponentType, int> PayrollComponentTypeRepository
        => transactionManager.GetRepository<PayrollComponentType, int>();

    private IAppRepository<PayrollInputType, int> PayrollInputTypeRepository
        => transactionManager.GetRepository<PayrollInputType, int>();

    private IAppRepository<PayrollInput, int> PayrollInputRepository
        => transactionManager.GetRepository<PayrollInput, int>();

    private IAppRepository<AllowanceRate, int> AllowanceRateRepository
        => transactionManager.GetRepository<AllowanceRate, int>();
    
    private IAppRepository<AllowanceCategory, int> AllowanceCategoryRepository
        => transactionManager.GetRepository<AllowanceCategory, int>();
    private IAppRepository<Dependent, int> DependentRepository
        => transactionManager.GetRepository<Dependent, int>();
    
    private IAppRepository<IncomeTaxBracket, int> IncomeTaxBracketRepository
        => transactionManager.GetRepository<IncomeTaxBracket, int>();
    
    private IAppRepository<DependentDeductionAmount, int> DependentRateRepository
        => transactionManager.GetRepository<DependentDeductionAmount, int>();
    
    private IAppRepository<InsuranceRateGroup, int> InsuranceRateGroupRepository
        => transactionManager.GetRepository<InsuranceRateGroup, int>();
    
    private IAppRepository<SelfDeductionAmount, int> SelfDeductionAmountRepository
        => transactionManager.GetRepository<SelfDeductionAmount, int>();
    #endregion
}