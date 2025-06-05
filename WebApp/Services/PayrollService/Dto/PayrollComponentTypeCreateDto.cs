namespace WebApp.Services.PayrollService.Dto;

public class PayrollComponentTypeCreateDto
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? DataSourceType { get; set; }
    public Guid? OrganizationId { get; set; }
    public int PayrollComponentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeductible { get; set; } = false;
    public bool IsTaxable { get; set; } = true;
    public int? InputTypeId { get; set; }
    public string? DataType { get; set; }
}
