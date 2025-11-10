using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.TaxDutyServices.Dto;

public class TaxDutyDto
{
    [MaxLength(500)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(0, 999999)]
    public int Order { get; set; }

    public int CategoryId { get; set; }
}

public class TaxDutyDisplayDto : TaxDutyDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}