namespace WebApp.Services.PayrollService.Dto;

public class PayrollComponentCategoryDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }
}