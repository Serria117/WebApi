namespace WebApp.Services.PayrollService.Dto;

public class ExcelPayrollDto
{
    public int Year { get; set; }
    public int Version { get; set; }
    public string FileName { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

public class ExcelPayrollDisplayDto
{
    public string Id { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Version { get; set; }
    public string FileName { get; set; } = string.Empty;
    public bool IsFinal { get; set; }
}

public class ExcelPayrollUpdateDto
{
    public string Id { get; set; } = string.Empty;
    public IFormFile File { get; set; } = null!;
}

public class ExcelPayrollDeleteDto
{
    public ICollection<string> Ids { get; set; } = [];
}