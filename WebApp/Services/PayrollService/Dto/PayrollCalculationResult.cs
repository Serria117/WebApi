using WebApp.Core.DomainEntities.Salary;

namespace WebApp.Services.PayrollService.Dto;

public class PayrollCalculationResult
{
    public decimal ActualWorkDays { get; set; }
    public decimal BaseSalaryOfActualWorkDays { get; set; }
    public TaxType TaxType { get; set; }
    public decimal NontaxableIncome { get; set; }
    public decimal SelfDeduction { get; set; }
    public int CountDependent { get; set; }
    public decimal DependentRate { get; set; }
    public List<InsuranceDetail>? Insurance { get; set; } = [];
    public decimal InsuranceDeduction { get; set; }
    public decimal CalculatedTaxableIncome { get; set; }
    public decimal IncomeTax { get; set; }
}

public class InsuranceDetail
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
}