namespace WebApp.Services.AccountingServices.Dto;

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
    public string? Status { get; set; }
    
}

public class FinancialReportDisplayDto
{
    public string Id { get; set; } = string.Empty;
    public Guid? OrganizationId { get; set; }
    public string Name { get; set; }  = string.Empty;
    public int Regulation { get; set; }
    public string? Note { get; set; } = string.Empty;
    public DateTime BeginDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime ReportDate { get; set; }
    public int Year { get; set; }
    public string FirstFiscalDate { get; set; } = string.Empty;
    public string TaxAgencyCode { get; set; } = string.Empty;
    public string TaxAgencyName { get; set; } = string.Empty;
    public string? Status { get; set; } = string.Empty;
    public string? XmlContent { get; set; } = string.Empty;
    public UserInputTrialDto? UserInput { get; set; }
    public TrialBalanceEntryDto[] TrialBalance { get; set; } = [];
    public IncomeStatementEntryDto[] IncomeStatement { get; set; } = [];
    public BalanceSheetEntryDto[] BalanceSheet { get; set; } = [];
}

public class UserInputTrialDto
{
    public int Id { get; set; }
    public UserInputEntryDto[] Entries { get; set; } = [];
}

public class UserInputEntryDto
{
    public long Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public decimal OpenCredit { get; set; }
    public decimal OpenDebit { get; set; }
    public decimal AriseCredit { get; set; }
    public decimal AriseDebit { get; set; }
    public decimal CloseCredit { get; set; }
    public decimal CloseDebit { get; set; }
    public bool IsMatchRegulation { get; set; }
    public bool IsUserInput { get; set; }
    public string? ParentCode { get; set; } = string.Empty;
    public bool? NetBalanceValid { get; set; }
    public decimal? InvalidNetBalanceDifference { get; set; }
}

public class TrialBalanceEntryDto
{
    public long Id { get; set; }
    public string? Name { get; set; } = string.Empty;
    public string AccountCode { get; set; } = string.Empty;
    public decimal OpenCredit { get; set; }
    public decimal OpenDebit { get; set; }
    public decimal AriseCredit { get; set; }
    public decimal AriseDebit { get; set; }
    public decimal CloseCredit { get; set; }
    public decimal CloseDebit { get; set; }
    public bool IsMatchRegulation { get; set; }
    public bool IsUserInput { get; set; }
    public string? ParentCode { get; set; } = string.Empty;
    public bool? NetBalanceValid { get; set; }
    public decimal? InvalidNetBalanceDifference { get; set; }
}

public class IncomeStatementEntryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal LastYear { get; set; }
    public decimal ThisYear { get; set; }
}

public class BalanceSheetEntryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public decimal BeginingBalance { get; set; }
    public decimal EndingBalance { get; set; }
}