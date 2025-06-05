namespace WebApp.Services.PayrollService.Dto;

public class PayrollComponentTypeDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? DataSourceType { get; set; }
    public Guid? OrganizationId { get; set; }
    public int PayrollComponentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeductible { get; set; }
    public bool IsTaxable { get; set; }
    public int? InputTypeId { get; set; }
    public string? DataType { get; set; }
}