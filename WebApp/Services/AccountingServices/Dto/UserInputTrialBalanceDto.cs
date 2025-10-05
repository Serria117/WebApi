using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Services.BalanceSheetService.Dto;

public class UserInputTrialBalanceDto
{
    public string FinancialReportId { get; set; } = string.Empty;
    public int Year { get; set; }
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Regulation { get; set; }
    public ICollection<UserBalanceEntryInput> BalanceEntries { get; set; } = [];
}

public class UserInputExcelFile
{
    public string FinancialReportId { get; set; } = string.Empty;
    public required IFormFile File { get; set; }
}

public class UserBalanceEntryInput
{
    public string Code { get; set; } = string.Empty;
    public decimal OpenDebit { get; set; }
    public decimal OpenCredit { get; set; }
    public decimal AriseDebit { get; set; }
    public decimal AriseCredit { get; set; }
    public decimal CloseDebit { get; set; }
    public decimal CloseCredit { get; set; }
}