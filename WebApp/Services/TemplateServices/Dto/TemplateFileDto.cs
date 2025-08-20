namespace WebApp.Services.TemplateServices.Dto;

public class TemplateFileCreateDto
{
    public string Version { get; set; } = string.Empty;
    public string? VersionNote { get; set; }
    public IFormFile? FileToUpload { get; set; }
}

public class TemplateFileUploadDto
{
    public int TemplateId { get; set; }
    public required TemplateFileCreateDto TemplateFile { get; set; }
}

public class TemplateFileDisplayDto
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? VersionNote { get; set; }
    public DateTime UploadTime { get; set; }
}
