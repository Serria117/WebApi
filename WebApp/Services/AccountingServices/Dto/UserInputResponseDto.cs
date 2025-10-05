using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Accounting.FinancialStatement;

namespace WebApp.Services.BalanceSheetService.Dto;

/// <summary>
/// Return dto for user input balance sheet processing, including the processed balance sheet and any validation results.
/// </summary>
public class UserInputResponseDto
{
    public UserInputTrialBalance? UserInputTrialBalance { get; set; }
    public ICollection<UserInputValidationResult> ValidationResults { get; set; } = [];
}

public class UserInputValidationResult
{
    public TrialBalanceEntry? BalanceEntry { get; set; }
    public List<string> Errors { get; set; } = [];
}