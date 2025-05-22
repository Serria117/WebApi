using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.PayrollService.Dto;

public class PayrollComponentCategoryCreateDto
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Range(0, 9999)]
    public int Order { get; set; } = 0;
}