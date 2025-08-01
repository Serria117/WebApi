using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApp.Services.TaxProcedureService.dto;

public class TaxProcedureCreateDto
{
    [Required, Length(3,500)]
    public string Name { get; set; } = string.Empty;
    [Required, Length(3,50)]
    public string Code { get; set; } = string.Empty;
    [Range(0,10000)]
    public int Order { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}

public class TaxProcedureDisplayDto : TaxProcedureCreateDto
{
    public int Id { get; set; }
}
public class TaxProcedureUpdateDto : TaxProcedureCreateDto
{
    [Required]
    public int Id { get; set; }
}
public class InvalidTaxProcedureDto
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
    
    [JsonPropertyName("taxProcedure")]
    public TaxProcedureCreateDto TaxProcedure { get; set; } = new();
}
