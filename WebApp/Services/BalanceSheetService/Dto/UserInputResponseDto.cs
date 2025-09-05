using WebApp.Core.DomainEntities.Accounting;

namespace WebApp.Services.BalanceSheetService.Dto;

/// <summary>
/// Return dto for user input balance sheet processing, including the processed balance sheet and any validation results.
/// </summary>
public class UserInputResponseDto
{
    public UserInputBalancesheet? UserInputBalancesheet { get; set; }
    public ICollection<UserInputValidationResult> ValidationResults { get; set; } = [];
}

public class UserInputValidationResult
{
    public BalanceEntry? BalanceEntry { get; set; }
    public List<string> Errors { get; set; } = [];
}