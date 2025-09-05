namespace WebApp.Services.BalanceSheetService.Dto;

public class FinancialReportWorkDto
{
    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int Year { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReportDate { get; set; }
    public string FirstFiscalDate { get; set; } = string.Empty;
    public int Regulation { get; set; }
}