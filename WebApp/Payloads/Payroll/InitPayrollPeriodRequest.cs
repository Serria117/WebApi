using System.ComponentModel.DataAnnotations;
using WebApp.Enums.Payroll;

namespace WebApp.Payloads.Payroll;
/// <summary>
/// Contains parameters for initializing a payroll period: Year, NetWorkDays, and WeekendType.
/// </summary>
public class InitPayrollPeriodRequest
{
    [Range(1990, 2990, ErrorMessage = "Year must be between 1990 and 2990.")]
    public int Year { get; set; }

    [Range(1, 30, ErrorMessage = "Net work days must be between 1 and 31.")]
    public int? NetWorkDays { get; set; }

    public WeekendType WeekendType { get; set; }
}