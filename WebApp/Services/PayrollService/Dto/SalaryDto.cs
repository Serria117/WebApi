using System.ComponentModel.DataAnnotations.Schema;
using WebApp.Core.DomainEntities.Payroll;

namespace WebApp.Services.PayrollService.Dto;

public class SalaryCreate
{
    public decimal SalaryValue { get; set; }
    public decimal? InsuranceSalaryValue { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public long EmployeeId { get; set; }
}

public class SalaryDisplay
{
    public long Id { get; set; }
    public decimal SalaryValue { get; set; }
    public decimal? InsuranceSalaryValue { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
}
