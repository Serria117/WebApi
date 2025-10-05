using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.UserService;

namespace WebApp.Services.PayrollService;

public interface IPayrollAppService
{
    Task<ResponseBase> CreatePayrollPeriodsAsync(int year, Weekend weekend);
    Task<ResponseBase> CreateEmployeeAsync(EmployeeCreateDto dto);
    Task<ResponseBase> GetEmployeesAsync(EmployeeQuery query);
    Task<ResponseBase> GetEmployeesInPeriod(long periodId);
    Task<ResponseBase> GetPayrollPeriodAsync(long pId);
    Task<ResponseBase> GetYearPayrollPeriod(PayrollPeriodQuery query);
    Task<ResponseBase> UpdateEmployeeAsync(long id, EmployeeUpdate dto);
    Task<ResponseBase> DeleteEmployeesAsync(List<long> idList);
    Task<ResponseBase> AddSalaryToEmployeeAsync(SalaryCreate dto);
    Task<ResponseBase> GetEmployeeById(long id);
    Task<ResponseBase> EditDependentsInEmployeeAsync(long employeeId, ICollection<DependentCreateDto> dtoList);
    Task<ResponseBase> CreateAllowanceType(AllowanceTypeCreate dto);
    Task<ResponseBase> GetAllowanceTypes();
    Task<ResponseBase> CreateTimesheetsAsync(long periodId);
    Task CreateDepartmentAsync(DepartmentCreate dto);
    Task<ResponseBase> GetDepartmentsAsync();
    Task<ResponseBase> GetDepartmentByIdAsync(string id);
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