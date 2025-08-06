using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.RegionService.Dto;

public class TaxOffice2CreateDto
{
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? ShortName { get; set; }

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;

    public int? ProvinceId { get; set; }
}

public class TaxOffice2DiplayDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string Code { get; set; } = string.Empty;
    public int? ProvinceId { get; set; }
    public string? ProvinceName { get; set; }
}