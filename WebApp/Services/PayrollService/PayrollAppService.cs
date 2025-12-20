using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Payroll;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.PayrollService.Dto;
using WebApp.Services.UserService;

namespace WebApp.Services.PayrollService;

public interface IPayrollAppService
{
    Task<ResponseEntity> CreatePayrollPeriodsAsync(int year, Weekend weekend);
    Task<ResponseEntity> CreateEmployeeAsync(EmployeeCreateDto dto);
    Task<ResponseEntity> GetEmployeesAsync(EmployeeQuery query);
    Task<ResponseEntity> GetEmployeesInPeriod(long periodId);
    Task<ResponseEntity> GetPayrollPeriodAsync(long pId);
    Task<ResponseEntity> GetYearPayrollPeriod(PayrollPeriodQuery query);
    Task<ResponseEntity> UpdateEmployeeAsync(long id, EmployeeUpdate dto);
    Task<ResponseEntity> DeleteEmployeesAsync(List<long> idList);
    Task<ResponseEntity> AddSalaryToEmployeeAsync(SalaryCreate dto);
    Task<ResponseEntity> GetEmployeeById(long id);
    Task<ResponseEntity> EditDependentsInEmployeeAsync(long employeeId, ICollection<DependentCreateDto> dtoList);
    Task<ResponseEntity> CreateAllowanceType(AllowanceTypeCreate dto);
    Task<ResponseEntity> GetAllowanceTypes();
    Task<ResponseEntity> CreateTimesheetsAsync(long periodId);
    Task CreateDepartmentAsync(DepartmentCreate dto);
    Task<ResponseEntity> GetDepartmentsAsync(RequestParam requestParam);
    Task<ResponseEntity> GetDepartmentByIdAsync(string id);
    Task<bool> IsDepartmentExistAsync(string name);
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
        => transaction.GetRepository<Timesheet, string>();

    private IAppRepository<Department, string> DepartmentRepository
        => transaction.GetRepository<Department, string>();
}