namespace WebApp.Payloads.Payroll;

/// <summary>
/// Contains the request parameters for retrieving payroll periods: Year, Month, and Version.
/// </summary>
public class PayrollPeriodRequest
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    
    public int? Version { get; set; }
}