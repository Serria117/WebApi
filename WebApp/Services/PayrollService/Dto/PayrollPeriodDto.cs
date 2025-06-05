using System.ComponentModel.DataAnnotations;
using WebApp.Enums.Payroll;

namespace WebApp.Services.PayrollService.Dto;

public class PayrollPeriodCreateDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    [Range(1D, 31D, ErrorMessage = "Net work days must be between 1 and 31.")]
    public decimal NetWorkDays { get; set; }

    [Range(1990, 2990, ErrorMessage = "Year must be between 1990 and 2990.")]
    public int Year { get; set; }

    [Range(1, 12, ErrorMessage = "Month must be between 1 and 12.")]
    public int Month { get; set; }

    public int Version { get; set; }
    public bool IsFinal { get; set; }
    public WeekendType WeekendType { get; set; } = WeekendType.Sunday;
}

public class PayrollPeriodDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal NetWorkDays { get; set; }
    public bool IsClosed { get; set; } = false;
    public int Year { get; set; }
    public int Month { get; set; }

    public int Version { get; set; }
    public bool IsFinal { get; set; }
    public string WeekendType { get; set; } = nameof(Enums.Payroll.WeekendType.Sunday);
}