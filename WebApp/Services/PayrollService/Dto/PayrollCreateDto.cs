namespace WebApp.Services.PayrollService.Dto;

public class PayrollCreateDto
{
    public required Guid EmployeeId { get; set; }
    public required decimal Amount { get; set; }
    public required DateTime PayDate { get; set; }
    public string? Notes { get; set; }
}