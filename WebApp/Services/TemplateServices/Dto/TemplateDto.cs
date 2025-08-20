using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.TemplateServices.Dto;

public class TemplateCreateDto
{
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Range(0, 10000)]
    public int Order { get; set; }

    public ICollection<TemplateFileCreateDto> TemplateFiles { get; set; } = [];
}

public class TemplateDisplayDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public List<TemplateFileDisplayDto> TemplateFiles { get; set; } = [];
}
