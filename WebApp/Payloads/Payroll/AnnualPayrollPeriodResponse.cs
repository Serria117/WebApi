using WebApp.Services.PayrollService.Dto;

namespace WebApp.Payloads.Payroll;

public class AnnualPayrollPeriodResponse
{
    public int Year { get; set; }
    public List<PayrollPeriodDisplayDto> PayrollPeriods { get; set; } = [];
}