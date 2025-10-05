using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Accounting.FinancialStatement;

namespace WebApp.Services.BalanceSheetService.Dto;

/// <summary>
/// Wrapper for user input trial balance mapping result
/// </summary>
public class TrialBalanceMapResult
{
    /// <summary>
    /// The collection of user input that has error
    /// </summary>
    public List<TrialBalanceEntry> ErrorInputEntry { get; set; } = [];
    
    /// <summary>
    /// The collection of report's trial balance that was not mapped.
    /// </summary>
    public List<TrialBalanceEntry> UnMappedEntry { get; set; } = [];
}