using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spire.Xls;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Enums;
using WebApp.Enums.Accounting;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;

namespace WebApp.Services.BalanceSheetService;

public interface IBalanceSheetAppService
{
    Task<AppResponse> GetAccountList(int regulationId);
    Task<AppResponse> CreateUserInputBalancesheet(UserInputBalancesheetDto input);
    Task CreateFinancialReportWork(FinancialReportWorkDto dto);
    Task<AppResponse> GetFinancialReportList();
    Task<AppResponse> MapUserBalancesheet(int userInputId);
}

public class BalanceSheetAppService(IAppRepository<Account, int> accountRepo,
                                    IAppRepository<Balancesheet, int> balancesheetRepo,
                                    IAppRepository<Organization, Guid> orgRepo,
                                    IAppRepository<FinancialReportWork, int> reportRepo,
                                    IAppRepository<UserInputBalancesheet, int> userBalancesheetRepo,
                                    IUserManager userManager) : BaseAppService(userManager), IBalanceSheetAppService
{
    public async Task<AppResponse> GetAccountList(int regulationId)
    {
        var result = await accountRepo.Find(a => a.AccountingRegulationId == regulationId)
                                      .OrderBy(a => a.Code)
                                      .ToListAsync();
        return AppResponse.OkResult(result.Select(a => new
        {
            a.Id,
            a.Name,
            a.Code,
            a.AccountingRegulationId,
            a.ParentCode,
            a.AccountBehavior,
            a.AccountType
        }));
    }

    public async Task CreateFinancialReportWork(FinancialReportWorkDto dto)
    {
        var organization = await orgRepo.Find(o => o.Id == WorkingOrg.ToGuid() && !o.Deleted)
                                        .Include(o => o.TaxOffice2)
                                        .FirstOrDefaultAsync() 
                           ?? throw new NotFoundException("Organization not found");
        var reportWork = new FinancialReportWork
        {
            OrganizationId = WorkingOrg.ToGuid(),
            Year = dto.Year,
            Name = dto.Name,
            FirstFiscalDate = dto.FirstFiscalDate,
            BeginDate = dto.BeginDate,
            EndDate = dto.EndDate,
            Note = dto.Note,
            Regulation = dto.Regulation,
            TaxAgencyCode = organization.TaxOffice2!.Code,
            TaxAgencyName = organization.TaxOffice2.FullName
        };
        await reportRepo.CreateAsync(reportWork);
    }

    public async Task<AppResponse> GetFinancialReportList()
    {
        var result = await reportRepo.Find(r => r.OrganizationId == WorkingOrg.ToGuid() && !r.Deleted)
                                     .Include(r => r.Balancesheet).ThenInclude(b => b!.AccountBalances)
                                     .AsSplitQuery()
                                     .AsNoTracking()
                                     .ToListAsync();
        return AppResponse.OkResult(result);
    }

    public async Task<AppResponse> GetFinancialReportById(int id)
    {
        var result = await reportRepo.Find(r => r.Id == id && r.OrganizationId == WorkingOrg.ToGuid() && !r.Deleted)
                                     .Include(r => r.Balancesheet)
                                     .ThenInclude(b => b!.AccountBalances)
                                     .Include(r => r.UserInput)
                                     .ThenInclude(u => u!.AccountBalances)
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync();
        return result is null 
            ? AppResponse.Error404("Id not found") 
            : AppResponse.OkResult(result);
    }
    
    //This method will save the user's input balancesheet to database for further processing
    //The user's input should be checked for validation and saved no matterwhat it is valid or not
    //The return value should contain the result of the validation to inform user
    //A newly created balancesheet data must be attached to an existing FinancialReportWork entity
    public async Task<AppResponse> CreateUserInputBalancesheet(UserInputBalancesheetDto input)
    {
        var report = await reportRepo.Find(r => r.Id == input.FinancialReportId && !r.Deleted)
                                     .Include(r => r.UserInput)
                                     .FirstOrDefaultAsync() ??
                     throw new NotFoundException("Financial report not found");

        var accounts = await accountRepo.Find(x => x.AccountingRegulationId == report.Regulation)
                                        .AsNoTracking()
                                        .ToListAsync();

        List<BalanceEntry> balanceEntries = [];
        Dictionary<string, List<string>> errorAccount = [];
        errorAccount.Add("AccountNotExist", []);
        errorAccount.Add("AccountValueNotValid", []);
        foreach (var inputEntry in input.BalanceEntries)
        {
            var entry = new BalanceEntry
            {
                AccountCode = inputEntry.Code,
                OpenDebit = inputEntry.OpenDebit,
                OpenCredit = inputEntry.OpenCredit,
                AriseDebit = inputEntry.AriseDebit,
                AriseCredit = inputEntry.AriseCredit,
                CloseCredit = inputEntry.CloseCredit,
                CloseDebit = inputEntry.CloseDebit,
            };
            balanceEntries.Add(entry);
            if (!AccountInputNotExist(entry.AccountCode, accounts))
            {
                errorAccount["AccountNotExist"].Add(entry.AccountCode);
            }
        }

        var newBalancesheet = new UserInputBalancesheet
        {
            OrganizationId = WorkingOrg.ToGuid(),
            AccountBalances = balanceEntries,
            Year = input.Year,
            Regulation = input.Regulation
        };

        report.UserInput = newBalancesheet;

        var updateResult = await reportRepo.UpdateAsync(report);

        return new AppResponse
        {
            Code = "200",
            Success = true,
            Data = new
            {
                Error = errorAccount
            }
        };
    }

    //This method with find the saved user input balancesheet and try to map it with a new system balancesheet.
    public async Task<AppResponse> MapUserBalancesheet(int userInputId)
    {
        var userInput = await userBalancesheetRepo.Find(b => b.Id == userInputId)
                                                  .Include(b => b.AccountBalances)
                                                  .FirstOrDefaultAsync();
        if (userInput is null) return AppResponse.Error404("Id not found");

        var accountsForMapping = await accountRepo
                                       .Find(a => a.AccountingRegulationId == userInput.Regulation && !a.Deleted)
                                       .ToListAsync();

        foreach (var inputEntries in userInput.AccountBalances)
        {
            var accountCode = accountsForMapping.FirstOrDefault(a => a.Code == inputEntries.AccountCode);
            if (accountCode is null)
            {
                continue;
            }
        }

        return AppResponse.Ok();
    }

    /// <summary>
    /// Extract the user input excel file and return the list of account balances.
    /// </summary>
    /// <param name="input">The body parameters that contains the xlsx file to extract data.</param>
    /// <returns></returns>
    public async Task<AppResponse> ExtractUserInputExcelFile(UserInputExcelFile input)
    {
        var report = await reportRepo.Find(r => r.Id == input.FinancialReportId && !r.Deleted)
                                     .Include(r => r.UserInput)
                                     .ThenInclude(u => u!.AccountBalances)
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync() ??
                     throw new NotFoundException("Financial report not found");
        
        //Retrieve the account list assosiated with the regulation
        var accountList = await accountRepo.Find(a => a.AccountingRegulationId == report.Regulation)
                                           .ToListAsync();
        var workbook = new Workbook();
        workbook.LoadFromStream(input.File.OpenReadStream());
        var sheet = workbook.Worksheets[0];
        var lastRow = sheet.LastRow;
        List<BalanceEntry> balanceEntries = [];
        List<UserInputValidationResult> validationResults = [];
        for (int row = 2; row <= lastRow; row++)
        {
            var entry = new BalanceEntry
            {
                AccountCode = sheet.Range[row, 1].Value2.ToString() ?? string.Empty,
                Name = sheet.Range[row, 2].Value2.ToString() ?? string.Empty,
                OpenDebit = sheet.Range[row, 3].Value2.ToString().ToDecimal(),
                OpenCredit = sheet.Range[row, 4].Value2.ToString().ToDecimal(),
                AriseDebit = sheet.Range[row, 5].Value2.ToString().ToDecimal(),
                AriseCredit = sheet.Range[row, 6].Value2.ToString().ToDecimal(),
                CloseDebit = sheet.Range[row, 7].Value2.ToString().ToDecimal(),
                CloseCredit = sheet.Range[row, 8].Value2.ToString().ToDecimal(),
                IsUserInput = true
            };
            //TODO check for valid entry
            var accountResult = new UserInputValidationResult
            {
                BalanceEntry = entry,
                Errors = []
            };
            
            if (AccountInputNotExist(entry.AccountCode, accountList))
            {
                accountResult.Errors.Add("AccountNotExist");
            }
            //Validate more conditions
            
            
            if(accountResult.Errors.Count > 0) //Only add to the result if there is any error
            {
                validationResults.Add(accountResult);
            }
            
            balanceEntries.Add(entry);
            if(entry.AccountCode == "911") break; //Assume that 911 is the last account in the user input
        }

        if (balanceEntries.Count > 0)
        {
            if (report.UserInput == null)
            {
                report.UserInput = new UserInputBalancesheet
                {
                    OrganizationId = WorkingOrg.ToGuid(),
                    Year = report.Year,
                    Regulation = report.Regulation,
                    AccountBalances = balanceEntries
                };
            }
            else
            {
                //TODO Decide whether to append or replace the existing data
                foreach (var entry in balanceEntries)
                {
                    
                }
            }
        }
        await reportRepo.UpdateAsync(report);
        return AppResponse.OkResult(new UserInputResponseDto
        {
            UserInputBalancesheet = report.UserInput,
            ValidationResults = validationResults
        });
    }

    private bool AccountInputNotExist(string code, List<Account> accounts)
    {
        return accounts.Any(a => a.Code == code);
    }

    private bool AccountBalanceDebitValid(BalanceEntry entry, Account account)
    {
        return entry.CloseDebit == CalculateCloseDebit(entry, account);
    }

    private bool AccountBalanceCreditValid(BalanceEntry entry, Account account)
    {
        return entry.CloseCredit == CalculateCloseCredit(entry, account);
    }

    private decimal CalculateCloseDebit(BalanceEntry entry, Account account)
    {
        if (account.AccountType is AccountType.Asset or AccountType.Expense)
        {
            var value = entry.OpenDebit + entry.AriseDebit - entry.OpenCredit - entry.AriseDebit;
            return Math.Max(value, 0);
        }

        return 0;
    }

    private decimal CalculateCloseCredit(BalanceEntry entry, Account account)
    {
        return 0;
    }
}