using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.UserService;

namespace WebApp.Services.PayrollService;

public interface IPayrollAppService
{
    Task<AppResponse> CreatePayrollPeriodsAsync(int year, Weekend weekend);
    Task<AppResponse> CreateEmployeeAsync(EmployeeCreateDto dto);
    Task<AppResponse> GetEmployeesAsync(EmployeeQuery query);
    Task<AppResponse> GetEmployeesInPeriod(long periodId);
    Task<AppResponse> GetPayrollPeriodAsync(long pId);
    Task<AppResponse> GetYearPayrollPeriod(PayrollPeriodQuery query);
    Task<AppResponse> UpdateEmployeeAsync(long id, EmployeeUpdate dto);
    Task<AppResponse> DeleteEmployeesAsync(List<long> idList);
    Task<AppResponse> AddSalaryToEmployeeAsync(SalaryCreate dto);
    Task<AppResponse> GetEmployeeById(long id);
    Task<AppResponse> EditDependentsInEmployeeAsync(long employeeId, ICollection<DependentCreateDto> dtoList);
    Task<AppResponse> CreateAllowanceType(AllowanceTypeCreate dto);
    Task<AppResponse> GetAllowanceTypes();
    Task<AppResponse> CreateTimesheetsAsync(long periodId);
    Task CreateDepartmentAsync(DepartmentCreate dto);
    Task<AppResponse> GetDepartmentsAsync();
    Task<AppResponse> GetDepartmentByIdAsync(string id);
}

public partial class PayrollAppService(IUserManager userManager,
                                       IUnitOfWork transaction,
                                       ILogger<PayrollAppService> logger)
    : BaseAppService(userManager), IPayrollAppService
{
    private IAppRepository<Organization, Guid> OrganizationRepository
        => transaction.GetRepository<Organization, Guid>();

    private IAppRepository<Employee, long> EmployeeRepository
        => transaction.GetRepository<Employee, long>();

    private IAppRepository<PayrollPeriod, long> PayrollPeriodRepository
        => transaction.GetRepository<PayrollPeriod, long>();

    private IAppRepository<Salary, long> SalaryRepository
        => transaction.GetRepository<Salary, long>();

    private IAppRepository<Dependents, long> DependentsRepository
        => transaction.GetRepository<Dependents, long>();

    private IAppRepository<Allowance, long> AllowanceRepository
        => transaction.GetRepository<Allowance, long>();

    private IAppRepository<AllowanceType, long> AllowanceTypesRepository
        => transaction.GetRepository<AllowanceType, long>();
    
    private IAppRepository<Timesheet, string> TimesheetRepository
        => transaction.GetRepository<Timesheet, String>();

    private IAppRepository<Department, string> DepartmentRepository
        => transaction.GetRepository<Department, string>();
}