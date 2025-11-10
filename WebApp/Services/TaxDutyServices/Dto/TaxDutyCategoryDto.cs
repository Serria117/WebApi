using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.TaxDutyServices.Dto;

public class TaxDutyCategoryDto
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;
}

public class TaxDutyCategoryDisplayDto : TaxDutyCategoryDto
{
    public int Id { get; set; }
    public ICollection<TaxDutyDisplayDto> TaxDutyRecords { get; set; } = [];
}