namespace WebApp.Services.PayrollService.Dto;

public class PayrollPeriodCreateDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}