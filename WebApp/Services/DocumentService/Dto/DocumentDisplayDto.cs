using WebApp.Enums;

namespace WebApp.Services.DocumentService.Dto;

public class DocumentDisplayDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadTime { get; set; }
    public DocumentType DocumentType { get; set; }
    public int? Period { get; set; }

    public int Year { get; set; }
    public int NumberOfAdjustment { get; set; } = 0;
    public string? PeriodType { get; set; } = string.Empty;
    public DateTime DocumentDate { get; set; }
    public string? AdjustmentType { get; set; }

    public string? PeriodString => $"{Period}/{Year}";
}
