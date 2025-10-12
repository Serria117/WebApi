using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Spire.Xls;
using WebApp.Core.Data;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting;
using WebApp.Core.DomainEntities.Accounting.FinancialStatement;
using WebApp.Enums.Accounting;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Repositories;
using WebApp.Services.AccountingServices.Dto;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.UserService;
using WebApp.Utils;
using ValueType = WebApp.Core.DomainEntities.Accounting.FinancialStatement.ValueType;

namespace WebApp.Services.AccountingServices;

public interface IFinancialStatementAppService
{
    Task<ResponseBase> GetAccountList(int regulationId);
    Task<ResponseBase> CreateUserInputTrialBalance(UserInputTrialBalanceDto input);
    Task CreateFinancialReportWork(FinancialReportWorkDto dto);
    Task<ResponseBase> GetFinancialReportList(int? fromYear, int? toYear);

    /// <summary>
    /// Get a single Financial Report to display
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<ResponseBase> GetFinancialReportById(string id);

    Task<ResponseBase> MapUserTrialBalance(string reportId);

    /// <summary>
    /// Extract the user input excel file and return the list of trial balance entries.<br/>
    /// </summary>
    /// <param name="input">The body parameters that contains the xlsx file to extract data.</param>
    /// <returns></returns>
    Task<ResponseBase> ImportUserInputTrialBalanceFromExcelFile(UserInputExcelFile input);

    Task<ResponseBase> MapIncomeStatementFromTrialBalance(string reportId);
    Task<ResponseBase> GetLastYearReports(int curentReportId);
    Task<ResponseBase> ClearAllUserInputTrialEntries(string reportId);
    Task<ResponseBase> SoftDeleteReport(string reportId);
    Task<ResponseBase> HardDeleteReport(string reportId);
    Task<ResponseBase> UpdateUserInputTrialBalance(string reportId, List<UserBalanceEntryUpdate> entries);
    Task<ResponseBase> DeleteUserInputTrialBalance(long[] ids);
    Task<ResponseBase> GetRegulationList();
    Task<ResponseBase> ResetReport(string reportId);

    /// <summary>
    /// Reset the Income Statement's values to zezo
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    Task<ResponseBase> ResetIncomeStatement(string reportId);

    Task<(string FileName, byte[] File)> DownloadXmlDocument(string reportId);
    Task<ResponseBase> CreateOrUpdateXmlDocument(string reportId);
    Task<ResponseBase> CalculateFinancialStatement(string reportId);
    Task<(string FileName, byte[] File)> ExportReportNoteExcel(string reportId);
    Task<(string FileName, byte[] File)> DownloadTrialBalanceTemplate();
}

public class FinancialStatementAppService(AppDbContext dbContext,
                                          IAppRepository<Account, int> accountRepo,
                                          IAppRepository<Organization, Guid> orgRepo,
                                          IAppRepository<FinancialReportWork, string> reportRepo,
                                          IAppRepository<TrialBalanceEntry, long> trialBalanceEntryRepo,
                                          IAppRepository<BalanceSheetMap, int> balancesheetMapRepo,
                                          IAppRepository<IncomeStatementMap, int> incomeStatementMapRepo,
                                          ILogger<FinancialStatementAppService> logger,
                                          IHostEnvironment env,
                                          IUserManager userManager)
    : BaseAppService(userManager), IFinancialStatementAppService
{
    private const string ExportTemplateFolder = "ExportTemplates";
    private const string ImportTemplateFOlder = "ImportTemplates";

    public async Task<ResponseBase> GetRegulationList()
    {
        var result = await dbContext.AccountingRegulations
                                    .Where(r => !r.Deleted 
                                                && r.RegulationType == RegulationType.FinancialReport)
                                    .Select(r => new
                                    {
                                        r.Id, r.Name, r.Description
                                    })
                                    .AsNoTracking()
                                    .ToListAsync();
        return ResponseBase.OkResult(result);
    }

    public async Task<ResponseBase> GetAccountList(int regulationId)
    {
        var result = await accountRepo.Find(a => a.AccountingRegulationId == regulationId && !a.Deleted)
                                      .OrderBy(a => a.Code)
                                      .AsNoTracking()
                                      .ToListAsync();
        return ResponseBase.OkResult(result.Select(a => new
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
                                        .Include(o => o.District).ThenInclude(d => d!.Province)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync()
                           ?? throw new NotFoundException("Organization not found");
        var accountsByRegulation = await accountRepo.Find(x => x.AccountingRegulationId == dto.Regulation)
                                                    .ToListAsync();
        var balanceSheetItems = await dbContext.BalanceSheetItems
                                               .Where(b => b.RegulationId == dto.Regulation && !b.Deleted)
                                               .ToListAsync();
        var incomeStatementItems = await dbContext.IncomeStatementItems
                                                  .Where(i => i.RegulationId == dto.Regulation && !i.Deleted)
                                                  .ToListAsync();
        List<TrialBalanceEntry> trialBalanceEntries = [];
        List<BalanceSheetEntry> balanceSheetEntries = [];
        List<IncomeStatementEntry> incomeStatementEntries = [];
        trialBalanceEntries.AddRange(accountsByRegulation.Select(acc => new TrialBalanceEntry()
        {
            AccountCode = acc.Code,
            Name = acc.Name,
            ParentCode = acc.ParentCode,
            OpenCredit = 0,
            OpenDebit = 0,
            AriseCredit = 0,
            AriseDebit = 0,
            CloseCredit = 0,
            CloseDebit = 0,
        }));
        balanceSheetEntries.AddRange(balanceSheetItems.Select(b => new BalanceSheetEntry()
        {
            BalanceSheetItem = b,
            Code = b.Code,
            Name = b.Name,
            BeginingBalance = 0,
            EndingBalance = 0,
            ParentCode = b.ParentCode
        }));
        incomeStatementEntries.AddRange(incomeStatementItems.Select(i => new IncomeStatementEntry()
        {
            IncomeStatementItem = i,
            Code = i.Code,
            Name = i.Name,
            ThisYear = 0,
            LastYear = 0
        }));

        var reportWork = new FinancialReportWork
        {
            OrganizationId = WorkingOrg.ToGuid(),
            Year = dto.Year,
            Name = dto.Name,
            FirstFiscalDate = dto.FirstFiscalDate,
            BeginDate = dto.BeginDate.ToLocalTime(),
            EndDate = dto.EndDate.ToLocalTime(),
            ReportDate = dto.ReportDate.ToLocalTime(),
            Note = dto.Note,
            Regulation = dto.Regulation,
            TaxAgencyCode = organization.TaxOffice2!.Code,
            TaxAgencyName = organization.TaxOffice2.FullName,
            District = organization.District?.Name,
            DistrictCode = organization.District?.Code,
            Province = organization.District?.Province?.AlterName,
            ProvinceCode = organization.District?.Province?.Code,
            TrialBalanceEntries = trialBalanceEntries,
            BalanceSheetEntries = balanceSheetEntries,
            IncomeStatementEntries = incomeStatementEntries,
            Xml = new ReportContentXml
            {
                FileName = $"{organization.TaxId}-BCTC-Y{dto.Year}-L00.xml",
                Content = string.Empty
            }
        };
        await reportRepo.CreateAsync(reportWork);
    }

    public async Task<ResponseBase> GetFinancialReportList(int? fromYear, int? toYear)
    {
        var query = reportRepo.Find(r => r.OrganizationId == WorkingOrg.ToGuid()
                                         && !r.Deleted);

        if (fromYear.HasValue)
        {
            query = query.Where(r => r.Year >= fromYear.Value);
        }

        if (toYear.HasValue)
        {
            query = query.Where(r => r.Year <= toYear.Value);
        }

        var result = await query.OrderByDescending(r => r.CreateAt)
                                .Select(r => new
                                {
                                    r.Id, r.Name,
                                    r.Note, r.Year,
                                    r.BeginDate, r.EndDate,
                                    r.Regulation,
                                    r.TaxAgencyName, r.TaxAgencyCode,
                                    r.CreateBy, r.CreateAt,
                                    r.Status, r.ReportDate
                                })
                                .AsNoTracking()
                                .ToListAsync();
        return ResponseBase.OkResult(result);
    }

    public async Task<ResponseBase> GetFinancialReportById(string id)
    {
        var result = await reportRepo.Find(r => r.Id == id 
                                                && r.OrganizationId == WorkingOrg.ToGuid() 
                                                && !r.Deleted)
                                     .Include(r => r.TrialBalanceEntries.OrderBy(e => e.AccountCode))
                                     .Include(r => r.BalanceSheetEntries.OrderBy(b => b.Code))
                                     .Include(r => r.IncomeStatementEntries.OrderBy(i => i.Code))
                                     .Include(r => r.UserInput).ThenInclude(i => i!.Entries.OrderBy(e => e.AccountCode))
                                     .Select(r => r.ToDisplayDto())
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync();
        return result is null
            ? ResponseBase.Error404("Id not found")
            : ResponseBase.OkResult(result);
    }


    /// <summary>
    /// - This method will save the user's input Trial Balance to database for further processing. <br/>
    /// - The user's input should be checked for validation and saved no matterwhat it is valid or not. <br/>
    /// - The return value should contain the result of the validation to inform user. <br/>
    /// - A newly created trial balance data must be attached to an existing FinancialReportWork entity.
    /// </summary>
    /// <param name="input">Input object contains report Id and collection of balance entries</param>
    /// <returns>Value object contain the validation result and the trial balance</returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<ResponseBase> CreateUserInputTrialBalance(UserInputTrialBalanceDto input)
    {
        var report = await reportRepo.Find(r => r.Id == input.FinancialReportId && !r.Deleted)
                                     .Include(r => r.UserInput).ThenInclude(u => u!.Entries)
                                     .FirstOrDefaultAsync() 
                     ?? throw new NotFoundException("Financial report not found");

        var accounts = await accountRepo.Find(x => x.AccountingRegulationId == report.Regulation)
                                        .AsNoTracking()
                                        .ToListAsync();

        List<TrialBalanceEntry> balanceEntries = [];
        Dictionary<string, List<string>> errorAccount = [];
        errorAccount.Add("accountNotExist",
        [
        ]); //List of account codes that do not exist in the chart of accounts according to the regulation
        errorAccount.Add("accountValueNotValid", []);
        foreach (var inputEntry in input.BalanceEntries)
        {
            var entry = new TrialBalanceEntry
            {
                AccountCode = inputEntry.Code,
                OpenDebit = inputEntry.OpenDebit,
                OpenCredit = inputEntry.OpenCredit,
                AriseDebit = inputEntry.AriseDebit,
                AriseCredit = inputEntry.AriseCredit,
                CloseCredit = inputEntry.CloseCredit,
                CloseDebit = inputEntry.CloseDebit,
                IsMatchRegulation = true
            };
            balanceEntries.Add(entry);
            if (!AccountInputNotExist(entry.AccountCode, accounts))
            {
                entry.IsMatchRegulation = false;
                errorAccount["accountNotExist"].Add(entry.AccountCode);
            }
        }

        var newBalancesheet = new UserInputTrialBalance
        {
            OrganizationId = WorkingOrg.ToGuid(),
            Entries = balanceEntries,
            Year = input.Year,
            Regulation = input.Regulation
        };

        report.UserInput = newBalancesheet;

        var updateResult = await reportRepo.UpdateAsync(report);

        return new ResponseBase
        {
            Code = "200",
            Success = true,
            Data = new
            {
                Error = errorAccount
            }
        };
    }

    /// <summary>
    /// Map user input trial balance to the report's trial balance table
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    public async Task<ResponseBase> MapUserTrialBalance(string reportId)
    {
        var report = await reportRepo.Find(r => r.Id == reportId && !r.Deleted)
                                     .Include(r => r.UserInput).ThenInclude(r => r!.Entries)
                                     .Include(r => r.TrialBalanceEntries)
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Financial report not found");

        if (report.UserInput == null || report.UserInput.Entries.Count == 0)
            throw new NotFoundException("Current report has no user input trial balance");

        var accountTemplates = await accountRepo.Find(a => a.AccountingRegulationId == report.UserInput.Regulation
                                                           && !a.Deleted)
                                                .ToListAsync();

        foreach (var inputEntries in report.UserInput.Entries)
        {
            var accountCode = accountTemplates.FirstOrDefault(a => a.Code == inputEntries.AccountCode);
            if (accountCode is null)
            {
                inputEntries.IsMatchRegulation = false;
                continue;
            }

            var matchEntry = report.TrialBalanceEntries
                                   .FirstOrDefault(x => x.AccountCode == inputEntries.AccountCode);
            if (matchEntry == null)
            {
                inputEntries.IsMatchRegulation = false;
                continue;
            }

            matchEntry.OpenDebit = inputEntries.OpenDebit;
            matchEntry.OpenCredit = inputEntries.OpenCredit;
            matchEntry.AriseDebit = inputEntries.AriseDebit;
            matchEntry.AriseCredit = inputEntries.AriseCredit;
            matchEntry.CloseCredit = inputEntries.CloseCredit;
            matchEntry.CloseDebit = inputEntries.CloseDebit;
            matchEntry.MappedSuccess = true;
        }

        var updatedReport = await reportRepo.UpdateAsync(report);

        var mappingResult = new TrialBalanceMapResult();

        mappingResult.ErrorInputEntry
                     .AddRange(report.UserInput.Entries
                                     .Where(entry => !entry.IsMatchRegulation));
        mappingResult.UnMappedEntry
                     .AddRange(report.TrialBalanceEntries
                                     .Where(entry => !entry.MappedSuccess));

        return ResponseBase.OkResult(new
        {
            Report = updatedReport.ToDisplayDto(),
            Errors = mappingResult
        });
    }

    /// <summary>
    /// Extract the user input excel file and return the list of trial balance entries.
    /// </summary>
    /// <param name="input">The body parameters that contains the xlsx file to extract data.</param>
    /// <returns>The colection of balance entries that were imported.</returns>
    public async Task<ResponseBase> ImportUserInputTrialBalanceFromExcelFile(UserInputExcelFile input)
    {
        var report = await reportRepo.Find(r => r.Id == input.FinancialReportId && !r.Deleted)
                                     .Include(r => r.UserInput)
                                     .ThenInclude(u => u!.Entries)
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync() ??
                     throw new NotFoundException("Financial report not found");
        var regulationId = report.Regulation;
        //Retrieve the account list assosiated with the regulation
        var accountList = await accountRepo.Find(a => a.AccountingRegulationId == regulationId)
                                           .ToListAsync();
        var workbook = new Workbook();
        workbook.LoadFromStream(input.File.OpenReadStream());
        var sheet = workbook.Worksheets[0];
        var lastRow = sheet.LastRow;
        List<TrialBalanceEntry> balanceEntries = [];
        for (int row = 2; row <= lastRow; row++)
        {
            var entry = new TrialBalanceEntry
            {
                AccountCode = sheet.Range[row, 1].Value2.ToString() ?? string.Empty,
                Name = sheet.Range[row, 2].Value2.ToString() ?? string.Empty,
                OpenDebit = sheet.Range[row, 3].Value2.ToString().ToDecimal().RoundToInteger(),
                OpenCredit = sheet.Range[row, 4].Value2.ToString().ToDecimal().RoundToInteger(),
                AriseDebit = sheet.Range[row, 5].Value2.ToString().ToDecimal().RoundToInteger(),
                AriseCredit = sheet.Range[row, 6].Value2.ToString().ToDecimal().RoundToInteger(),
                CloseDebit = sheet.Range[row, 7].Value2.ToString().ToDecimal().RoundToInteger(),
                CloseCredit = sheet.Range[row, 8].Value2.ToString().ToDecimal().RoundToInteger(),
                IsUserInput = true,
            };
            if (entry.AccountCode.Length > 3)
            {
                entry.ParentCode = entry.AccountCode[..^1];
            }

            //TODO check for valid entry
            var accountResult = new UserInputValidationResult
            {
                BalanceEntry = entry,
                Errors = []
            };

            if (AccountInputNotExist(entry.AccountCode, accountList))
            {
                entry.IsMatchRegulation = false;
                accountResult.Errors.Add("AccountNotExist");
            }
            else
            {
                var account = accountList.First(a => a.Code == entry.AccountCode);
                (bool result, decimal error) = CloseBalanceValid(entry, account);
                entry.NetBalanceValid = result;
                entry.InvalidNetBalanceDifference = error;
            }

            //Validate more conditions
            balanceEntries.Add(entry);
            if (entry.AccountCode == "911") break; //Assume that 911 is the last account in the user input
        }

        if (balanceEntries.Count > 0)
        {
            if (report.UserInput == null)
            {
                report.UserInput = new UserInputTrialBalance
                {
                    OrganizationId = WorkingOrg.ToGuid(),
                    Year = report.Year,
                    Regulation = report.Regulation,
                    Entries = balanceEntries
                };
                await reportRepo.UpdateAsync(report);
            }
            else
            {
                //Update existing entries or add new entries
                foreach (var entry in balanceEntries)
                {
                    var existingEntry = report.UserInput.Entries
                                              .FirstOrDefault(e => e.AccountCode == entry.AccountCode);
                    if (existingEntry != null)
                    {
                        existingEntry.OpenDebit = entry.OpenDebit;
                        existingEntry.OpenCredit = entry.OpenCredit;
                        existingEntry.AriseDebit = entry.AriseDebit;
                        existingEntry.AriseCredit = entry.AriseCredit;
                        existingEntry.CloseDebit = entry.CloseDebit;
                        existingEntry.CloseCredit = entry.CloseCredit;
                    }
                    else
                    {
                        report.UserInput.Entries.Add(entry);
                    }
                }
            }

            //Calculate and sum the values for parent accounts if they have children
            //SumForParentAccount(report.UserInput.Entries);
        }

        await reportRepo.UpdateAsync(report);

        return ResponseBase.OkResult(new
        {
            Entries = report.UserInput?.Entries.Select(e => new
            {
                e.Id,
                e.AccountCode, e.Name,
                e.OpenCredit, e.OpenDebit,
                e.AriseCredit, e.AriseDebit,
                e.CloseCredit, e.CloseDebit,
                e.IsMatchRegulation,
                e.NetBalanceValid,
                e.InvalidNetBalanceDifference
            }).ToArray(),
        });
    }

    public async Task<ResponseBase> DeleteUserInputTrialBalance(long[] ids)
    {
        var result = await trialBalanceEntryRepo.SoftDeleteManyAsync(ids);
        return ResponseBase.OkResult(result);
    }

    /// <summary>
    /// This method is used to update the existing user input trial balance entries.<br/>
    /// </summary>
    /// <param name="reportId">The report Id to modify. It must belong to the current working Organization.</param>
    /// <param name="entries">A collection of balance entry to be udpated. <br/>
    ///     Only the accounts present in the collection should be updated. <br/>
    ///     The other accounts should remain unchanged.
    /// </param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">will be throw if report Id does not exist.</exception>
    public async Task<ResponseBase> UpdateUserInputTrialBalance(string reportId,
                                                                List<UserBalanceEntryUpdate> entries)
    {
        try
        {
            var report = await reportRepo.Find(r => r.Id == reportId && !r.Deleted
                                                                     && r.OrganizationId == WorkingOrg.ToGuid())
                                         .Include(r => r.UserInput)
                                         .AsSplitQuery()
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync()
                         ?? throw new NotFoundException("Financial report not found");
            if(report.UserInput == null) throw new NotFoundException("Financial report has no user entries yet!");
            
            var updatingEntries = await dbContext.TrialBalanceEntries
                                                 .Where(t => entries.Select(e => e.Id).Contains(t.Id)
                                                        && t.UserInputTrialBalanceId == report.UserInput.Id
                                                        && t.IsUserInput)
                                                 .ToListAsync();
            if(updatingEntries.Count == 0) throw new NotFoundException("Entries not found.");

            int updateCount = 0;
            foreach (var trialBalanceEntry in updatingEntries)
            {
                var inputEntry = entries.FirstOrDefault(e => e.Id == trialBalanceEntry.Id);
                if (inputEntry == null)
                {
                    continue;
                }

                trialBalanceEntry.OpenDebit = inputEntry.OpenDebit;
                trialBalanceEntry.OpenCredit = inputEntry.OpenCredit;
                trialBalanceEntry.AriseDebit = inputEntry.AriseDebit;
                trialBalanceEntry.AriseCredit = inputEntry.AriseCredit;
                trialBalanceEntry.CloseDebit = inputEntry.CloseDebit;
                trialBalanceEntry.CloseCredit = inputEntry.CloseCredit;
                updateCount++;
            }
            if(updateCount == 0) throw new NotFoundException("No entries updated because not matched Id.");
            dbContext.TrialBalanceEntries.UpdateRange(updatingEntries);
            await dbContext.SaveChangesAsync();
            return ResponseBase.Ok();
        }
        catch (NotFoundException e)
        {
            logger.LogErrorFormatted(exception: e);
            throw;
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e,
                                     message: "Unexpected error has orcured on the server side.");
            throw;
        }
    }

    public async Task<ResponseBase> CalculateFinancialStatement(string reportId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var report = dbContext.FinancialReportWorks
                                  .Include(r => r.TrialBalanceEntries)
                                  .Include(r => r.BalanceSheetEntries)
                                  .Include(r => r.IncomeStatementEntries)
                                  .ThenInclude(ie => ie.IncomeStatementItem)
                                  .Include(r => r.Organization)
                                  .ThenInclude(o => o!.District).ThenInclude(d => d!.Province)
                                  .AsSplitQuery()
                                  .FirstOrDefault(r => r.Id == reportId
                                                       && !r.Deleted
                                                       && r.OrganizationId == WorkingOrg.ToGuid())
                         ?? throw new NotFoundException("Financial report not found");

            await MapBalanceSheetAsync(report);
            await MapIncomeStatementAsync(report);
            report.Status = ReportStatus.Ready;
            dbContext.FinancialReportWorks.Update(report);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return ResponseBase.OkResult(report);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ResponseBase> ResetReport(string reportId)
    {
        var report = await reportRepo.Find(r => r.Id == reportId && !r.Deleted
                                                                 && r.OrganizationId == WorkingOrg.ToGuid())
                                     .Include(r => r.BalanceSheetEntries)
                                     .Include(r => r.IncomeStatementEntries)
                                     .Include(r => r.TrialBalanceEntries)
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Financial report not found");

        var sumBl = report.BalanceSheetEntries.Sum(x => x.EndingBalance + x.BeginingBalance);
        var sumIncome = report.IncomeStatementEntries.Sum(x => x.ThisYear + x.LastYear);
        var sumTrial = report.TrialBalanceEntries.Sum(x => x.AriseCredit
                                                           + x.AriseDebit + x.OpenCredit
                                                           + x.CloseCredit + x.OpenDebit
                                                           + x.CloseDebit);
        //If all values equal zero, stop:
        if (sumBl + sumIncome + sumTrial == 0)
        {
            return ResponseBase.Ok();
        }

        foreach (var b in report.BalanceSheetEntries)
        {
            b.EndingBalance = 0;
            b.BeginingBalance = 0;
        }

        foreach (var i in report.IncomeStatementEntries)
        {
            i.LastYear = 0;
            i.ThisYear = 0;
        }

        foreach (var t in report.TrialBalanceEntries)
        {
            t.AriseCredit = 0;
            t.AriseDebit = 0;
            t.OpenCredit = 0;
            t.OpenDebit = 0;
            t.CloseCredit = 0;
            t.CloseDebit = 0;
        }

        await reportRepo.UpdateAsync(report);
        return ResponseBase.Ok();
    }

    public async Task<ResponseBase> MapIncomeStatementFromTrialBalance(string reportId)
    {
        //await MapIncomeStatementAsync(reportId);
        return ResponseBase.Ok();
    }


    /// <summary>
    /// Reset the Income Statement's values to zezo
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<ResponseBase> ResetIncomeStatement(string reportId)
    {
        var report = await reportRepo.Find(r => r.Id == reportId && r.OrganizationId == WorkingOrg.ToGuid())
                                     .Include(r => r.IncomeStatementEntries)
                                     .AsSplitQuery()
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Financial report not found");
        foreach (var entry in report.IncomeStatementEntries)
        {
            entry.ThisYear = 0;
            entry.LastYear = 0;
        }

        await reportRepo.UpdateAsync(report);
        return ResponseBase.Ok();
    }

    /// <summary>
    /// Provide a list of selectable reports to be assigned as last year report of a report. <br/>
    /// </summary>
    /// <param name="currentYear"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<ResponseBase> GetLastYearReports(int currentYear)
    {
        var lastYearReport = await reportRepo.Find(r => r.OrganizationId == WorkingOrg.ToGuid()
                                                        && r.Year == currentYear - 1
                                                        && !r.Deleted)
                                             .Select(r => new
                                             {
                                                 r.Id, r.Name, r.Note, r.BeginDate, r.EndDate,
                                                 r.ReportDate, r.CreateAt
                                             })
                                             .ToListAsync();
        return ResponseBase.OkResult(lastYearReport);
    }

    /// <summary>
    /// Assign the last year report for the current report.<br/>
    /// A report can only have one last year report at a time.
    /// </summary>
    /// <param name="curentReportId"></param>
    /// <param name="lastYearReportId"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<ResponseBase> SelectLastYearReport(string curentReportId, string lastYearReportId)
    {
        var reports = await reportRepo.Find(r => (r.Id == curentReportId || r.Id == lastYearReportId)
                                                 && !r.Deleted
                                                 && r.OrganizationId == WorkingOrg.ToGuid())
                                      .Include(r => r.IncomeStatementEntries)
                                      .Include(r => r.BalanceSheetEntries)
                                      .Include(r => r.TrialBalanceEntries)
                                      .AsSplitQuery()
                                      .ToListAsync();
        var lastYearReport = reports.FirstOrDefault(r => r.Id == lastYearReportId)
                             ?? throw new NotFoundException("Financial report not found");
        var currentReport = reports.FirstOrDefault(r => r.Id == curentReportId)
                            ?? throw new NotFoundException("Financial report not found");

        if (currentReport.IncomeStatementEntries.Count > 0 && lastYearReport.IncomeStatementEntries.Count > 0)
        {
            foreach (var currentEntry in currentReport.IncomeStatementEntries)
            {
                var matchEntry = lastYearReport.IncomeStatementEntries
                                               .FirstOrDefault(i => i.Code == currentEntry.Code);
                currentEntry.ThisYear = matchEntry?.ThisYear ?? 0;
            }
        }

        //TODO: check opening balances with last year closing balances in trial balance
        await reportRepo.UpdateAsync(currentReport);
        return ResponseBase.Ok();
    }


    public async Task<ResponseBase> SoftDeleteReport(string reportId)
    {
        var report = await reportRepo.Find(r => r.Id == reportId
                                                && !r.Deleted
                                                && r.OrganizationId == WorkingOrg.ToGuid())
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Report not found");
        report.Deleted = true;
        await reportRepo.UpdateAsync(report);
        return ResponseBase.Ok($"Report {report.Name} deleted successfully");
    }

    /// <summary>
    /// Physically delete the report and all its related entities from the database.<br/>
    /// </summary>
    /// <param name="reportId">The id of the report to be removed.</param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">If the report is not found in the database.</exception>
    public async Task<ResponseBase> HardDeleteReport(string reportId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var report = await dbContext.FinancialReportWorks.Where(r => r.Id == reportId)
                                        .Include(r => r.UserInput).ThenInclude(u => u!.Entries)
                                        .Include(r => r.TrialBalanceEntries)
                                        .Include(r => r.BalanceSheetEntries)
                                        .Include(r => r.IncomeStatementEntries)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync()
                         ?? throw new NotFoundException("Report not found");
            if (report.UserInput is not null)
            {
                dbContext.TrialBalanceEntries.RemoveRange(report.UserInput.Entries); //Remove all entries first
                dbContext.UserInputTrialBalance.Remove(report.UserInput); //Then remove the user input itself
            }

            dbContext.TrialBalanceEntries.RemoveRange(report.TrialBalanceEntries);
            dbContext.BalanceSheetEntries.RemoveRange(report.BalanceSheetEntries);
            dbContext.IncomeStatementEntries.RemoveRange(report.IncomeStatementEntries);
            dbContext.FinancialReportWorks.Remove(report); //Finally remove the report itself
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            await transaction.RollbackAsync();
            return ResponseBase.Error(
                "An error has orcured while attemping to delete the entities. " +
                "See inner exception for details.");
        }

        return ResponseBase.Ok();
    }

    public async Task<ResponseBase> ClearAllUserInputTrialEntries(string reportId)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var report = await dbContext.FinancialReportWorks
                                        .Where(r => r.Id == reportId
                                                    && r.OrganizationId == WorkingOrg.ToGuid())
                                        .Include(r => r.UserInput).ThenInclude(u => u!.Entries)
                                        .Include(r => r.TrialBalanceEntries)
                                        .Include(r => r.BalanceSheetEntries)
                                        .Include(r => r.IncomeStatementEntries)
                                        .AsSplitQuery()
                                        .FirstOrDefaultAsync() ?? throw new NotFoundException("Report not found");
            if (report.UserInput is null) throw new NotFoundException("User input not found");
            foreach (var entry in report.BalanceSheetEntries)
            {
                entry.EndingBalance = 0;
                entry.BeginingBalance = 0;
            }

            foreach (var entry in report.IncomeStatementEntries)
            {
                entry.ThisYear = 0;
                entry.LastYear = 0;
            }

            foreach (var entry in report.TrialBalanceEntries)
            {
                entry.OpenCredit = 0;
                entry.OpenDebit = 0;
                entry.AriseCredit = 0;
                entry.AriseDebit = 0;
                entry.CloseCredit = 0;
                entry.CloseDebit = 0;
            }

            report.Status = ReportStatus.Pending;
            dbContext.TrialBalanceEntries.RemoveRange(report.UserInput.Entries); //Remove all entries first
            dbContext.UserInputTrialBalance.Remove(report.UserInput); //Then remove the user input itself
            dbContext.FinancialReportWorks.Update(report);
            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            await transaction.RollbackAsync();
            return ResponseBase.Error(
                "An error has orcured while attemping to delete the entities. See inner exception for details.");
        }

        return ResponseBase.Ok();
    }

    public async Task<(string FileName, byte[] File)> DownloadXmlDocument(string reportId)
    {
        Console.WriteLine("ReportID:" + reportId);
        Console.WriteLine("OrganizationId: " + WorkingOrg.ToGuid());
        var report = await reportRepo.Find(x => x.Id == reportId
                                                && x.OrganizationId == WorkingOrg.ToGuid())
                                     .Include(x => x.Xml)
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Report not found");
        var fileName = $"{report.Name}-{report.Year}-L00.xml";
        if (report.Xml is null)
        {
            var content = await CreateXmlContent(reportId);
            var xml = new ReportContentXml
            {
                FileName = fileName,
                Content = content.ToString()
            };
            report.Xml = xml;
            await reportRepo.UpdateAsync(report);
        }

        var doc = XDocument.Parse(report.Xml.Content);
        await using var stream = new MemoryStream();
        doc.Save(stream);
        stream.Position = 0;
        var file = stream.ToArray();
        return (report.Xml?.FileName ?? fileName, file);
    }

    public async Task<ResponseBase> CreateOrUpdateXmlDocument(string reportId)
    {
        var report = await reportRepo.Find(x => x.Id == reportId
                                                && x.OrganizationId == WorkingOrg.ToGuid())
                                     .Include(x => x.Xml)
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Report not found");
        var content = await CreateXmlContent(reportId);
        //Create if not exist
        if (report.Xml is null)
        {
            var fileName = $"{report.Name}-{report.Year}-L00.xml";
            report.Xml = new ReportContentXml
            {
                FileName = fileName,
                Content = content.ToString()
            };
            await reportRepo.UpdateAsync(report);
            return ResponseBase.Ok();
        }

        //Update content if exist
        report.Xml.Content = content.ToString();
        await reportRepo.UpdateAsync(report);
        return ResponseBase.Ok();
    }

    public async Task<(string FileName, byte[] File)> ExportReportNoteExcel(string reportId)
    {
        var report = await dbContext.FinancialReportWorks
                                    .Where(r => r.Id == reportId
                                                && r.OrganizationId == WorkingOrg.ToGuid())
                                    .Include(r => r.TrialBalanceEntries)
                                    .Include(r => r.Organization)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync();
        if (report is null) throw new NotFoundException("Report not found");
        var resultExcel = await CreateFinancialStatementNoteExcel(report);
        return resultExcel;
    }

    public async Task<(string FileName, byte[] File)> DownloadTrialBalanceTemplate()
    {
        const string filename = "Template_bang_can_doi_tk.xlsx";
        var templateFile = LoadExcelTemplate(ImportTemplateFOlder, filename); 
        await using var stream = new MemoryStream();
        templateFile.SaveToStream(stream, FileFormat.Version2016);
        var file = stream.ToArray();
        return (filename, file);
    }

    #region Private Method

    private static bool AccountInputNotExist(string code, List<Account> accounts)
    {
        return !accounts.Select(x => x.Code).Contains(code);
    }

    /// <summary>
    /// Validate account entry's closing banlance.
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="account"></param>
    /// <returns></returns>
    private static (bool Result, decimal Error) CloseBalanceValid(TrialBalanceEntry entry, Account account)
    {
        bool result = true;
        decimal error = 0;
        if (account.AccountBehavior == 0)
        {
            if (account.AccountType == AccountType.Asset)
            {
                var checkValue = entry.OpenCredit + entry.AriseCredit - entry.OpenDebit - entry.AriseDebit;
                result = entry.CloseCredit == checkValue && entry.CloseDebit == 0;
                error = entry.CloseCredit - checkValue;
            }

            if (account.AccountType == AccountType.Liability)
            {
                var checkValue = entry.OpenDebit + entry.AriseDebit - entry.OpenCredit - entry.AriseCredit;
                result = entry.CloseDebit == checkValue && entry.CloseCredit == 0;
                error = entry.CloseDebit - checkValue;
            }
        }

        if (account.AccountBehavior == 1)
        {
            var netBalance = Math.Abs(entry.OpenCredit + entry.AriseCredit - entry.OpenDebit - entry.AriseDebit);
            result = Math.Abs(entry.CloseDebit - entry.CloseCredit) == netBalance;
            error = netBalance - Math.Abs(entry.CloseDebit - entry.CloseCredit);
        }

        return (result, error);
    }

    private async Task<XDocument> CreateXmlContent(string reportId)
    {
        var report = await reportRepo.Find(r => r.Id == reportId)
                                     .Select(r => new
                                     {
                                         r.Id,
                                         r.Regulation, r.Year, r.BeginDate, r.EndDate, r.ReportDate,
                                         r.TaxAgencyCode, r.TaxAgencyName,
                                         r.Organization!.Address,
                                         Name = r.Organization.FullName,
                                         r.Organization.TaxId,
                                         Province = new
                                         {
                                             r.Organization.District!.Province!.Name,
                                             r.Organization.District!.Province!.Code
                                         },
                                         r.BalanceSheetEntries,
                                         r.IncomeStatementEntries,
                                         r.TrialBalanceEntries
                                     })
                                     .AsSplitQuery()
                                     .AsNoTracking()
                                     .FirstOrDefaultAsync()
                     ?? throw new NotFoundException("Report not found");
        var template = await dbContext.ReportTemplateXml
                                      .FirstOrDefaultAsync(t => t.RegulationId == report.Regulation)
                       ?? throw new NotFoundException("Template not found.");
        if (string.IsNullOrEmpty(template.XmlTemplate))
        {
            throw new NotFoundException("Template XML is empty.");
        }

        const string root = "HSoThueDTu/HSoKhaiThue/";
        var xmlDoc = XDocument.Parse(template.XmlTemplate);
        XNamespace ns = xmlDoc.Root?.Name.Namespace ?? XNamespace.None;
        var kyKKhaiThue =
            xmlDoc.GetChildElementByPath(root + "TTinChung/TTinTKhaiThue/TKhaiThue/KyKKhaiThue");
        kyKKhaiThue?.Element(ns + "kyKKhai")?.SetValue(report.Year.ToString());
        kyKKhaiThue?.Element(ns + "kyKKhaiTuNgay")?.SetValue(report.BeginDate.ToString("dd/MM/yyyy"));
        kyKKhaiThue?.Element(ns + "kyKKhaiTuNgay")?.SetValue(report.EndDate.ToString("dd/MM/yyyy"));

        var tTkhaiThue = xmlDoc.GetChildElementByPath(root + "TTinChung/TTinTKhaiThue/TKhaiThue");
        tTkhaiThue?.Element(ns + "maCQTNoiNop")?.SetValue(report.TaxAgencyCode);
        tTkhaiThue?.Element(ns + "tenCQTNoiNop")?.SetValue(report.TaxAgencyName);
        tTkhaiThue?.Element(ns + "ngayLapTKhai")?.SetValue(report.ReportDate.ToString("yyyy/MM/dd"));
        tTkhaiThue?.Element(ns + "ngayKy")?.SetValue(report.ReportDate.ToString("yyyy/MM/dd"));

        var nnt = xmlDoc.GetChildElementByPath(root + "TTinChung/TTinTKhaiThue/NNT");
        nnt?.Element(ns + "maTinhNNT")?.SetValue(report.Province.Code);
        nnt?.Element(ns + "tenTinhNNT")?.SetValue(report.Province.Name);
        nnt?.Element(ns + "tenNNT")?.SetValue(report.Name);
        nnt?.Element(ns + "mst")?.SetValue(report.TaxId);
        nnt?.Element(ns + "dchiNNT")?.SetValue(report.Address ?? String.Empty);

        var ngayLap = xmlDoc.GetChildElementByPath(root + "CTieuTKhaiChinh")?
            .Element(ns + "ngayLap");
        ngayLap?.SetValue(report.ReportDate.ToString("yyyy-MM-dd"));

        var chiTieuChinhCuoiNam = xmlDoc.GetChildElementByPath(root + "CTieuTKhaiChinh/SoCuoiNam");
        if (chiTieuChinhCuoiNam != null)
        {
            foreach (var child in chiTieuChinhCuoiNam.Descendants())
            {
                var bl = report.BalanceSheetEntries
                               .FirstOrDefault(x => ("ct" + x.Code).Equals(child.Name.LocalName,
                                                                           StringComparison
                                                                               .InvariantCultureIgnoreCase));
                if (bl == null)
                {
                    continue;
                }

                child.SetValue(bl.EndingBalance.ToString("F0"));
            }
        }

        var chiTieuChinhDauNam = xmlDoc.GetChildElementByPath(root + "CTieuTKhaiChinh/SoDauNam");
        if (chiTieuChinhDauNam != null)
        {
            foreach (var child in chiTieuChinhDauNam.Descendants())
            {
                var bl = report.BalanceSheetEntries
                               .FirstOrDefault(x => ("ct" + x.Code).Equals(child.Name.LocalName,
                                                                           StringComparison
                                                                               .InvariantCultureIgnoreCase));
                if (bl == null)
                {
                    continue;
                }

                child.SetValue(bl.BeginingBalance.ToString("F0"));
            }
        }

        var plKqkdNamNay = xmlDoc.GetChildElementByPath(root + "PLuc/PL_KQHDSXKD/NamNay");
        if (plKqkdNamNay != null)
        {
            foreach (var child in plKqkdNamNay.Descendants())
            {
                var incomeStatementEntry = report.IncomeStatementEntries
                                                 .FirstOrDefault(x => ("ct" + x.Code) == child.Name.LocalName);
                if (incomeStatementEntry == null) continue;
                child.SetValue(incomeStatementEntry.ThisYear.ToString("F0"));
            }
        }

        var plKqkdNamTruoc = xmlDoc.GetChildElementByPath(root + "PLuc/PL_KQHDSXKD/NamTruoc");
        if (plKqkdNamTruoc != null)
        {
            foreach (var child in plKqkdNamTruoc.Descendants())
            {
                var incomeStatementEntry = report.IncomeStatementEntries
                                                 .FirstOrDefault(x => ("ct" + x.Code) == child.Name.LocalName);
                if (incomeStatementEntry == null) continue;
                child.SetValue(incomeStatementEntry.LastYear.ToString("F0"));
            }
        }

        var cdtkNoDauKy = xmlDoc.GetChildElementByPath(root + "PLuc/PL_CDTK/SoDuDauKy/No");
        if (cdtkNoDauKy != null)
        {
            foreach (var child in cdtkNoDauKy.Descendants())
            {
                var entry = report.TrialBalanceEntries
                                  .FirstOrDefault(x => ("ct" + x.AccountCode).Equals(
                                                      child.Name.LocalName,
                                                      StringComparison.InvariantCultureIgnoreCase));
                if (entry == null) continue;
                child.SetValue(entry.OpenDebit.ToString("F0"));
            }
        }

        var cdtkCoDauKy = xmlDoc.GetChildElementByPath(root + "PLuc/PL_CDTK/SoDuDauKy/Co");
        if (cdtkCoDauKy != null)
        {
            foreach (var child in cdtkCoDauKy.Descendants())
            {
                var entry = report.TrialBalanceEntries
                                  .FirstOrDefault(x => ("ct" + x.AccountCode).Equals(
                                                      child.Name.LocalName,
                                                      StringComparison.InvariantCultureIgnoreCase));
                if (entry == null) continue;
                child.SetValue(entry.OpenCredit.ToString("F0"));
            }
        }

        var cdtkNoPs = xmlDoc.GetChildElementByPath(root + "PLuc/PL_CDTK/SoPhatSinhTrongKy/No");
        if (cdtkNoPs != null)
        {
            foreach (var child in cdtkNoPs.Descendants())
            {
                var entry = report.TrialBalanceEntries
                                  .FirstOrDefault(x => ("ct" + x.AccountCode).Equals(
                                                      child.Name.LocalName,
                                                      StringComparison.InvariantCultureIgnoreCase));
                if (entry == null) continue;
                child.SetValue(entry.AriseDebit.ToString("F0"));
            }
        }

        var cdtkCoPs = xmlDoc.GetChildElementByPath(root + "PLuc/PL_CDTK/SoPhatSinhTrongKy/Co");
        if (cdtkCoPs != null)
        {
            foreach (var child in cdtkCoPs.Descendants())
            {
                var entry = report.TrialBalanceEntries
                                  .FirstOrDefault(x => ("ct" + x.AccountCode).Equals(
                                                      child.Name.LocalName,
                                                      StringComparison.InvariantCultureIgnoreCase));
                if (entry == null) continue;
                child.SetValue(entry.AriseCredit.ToString("F0"));
            }
        }

        var cdtkNoCuoiKy = xmlDoc.GetChildElementByPath(root + "PLuc/PL_CDTK/SoDuCuoiKy/No");
        if (cdtkNoCuoiKy != null)
        {
            foreach (var child in cdtkNoCuoiKy.Descendants())
            {
                var entry = report.TrialBalanceEntries
                                  .FirstOrDefault(x => ("ct" + x.AccountCode).Equals(
                                                      child.Name.LocalName,
                                                      StringComparison.InvariantCultureIgnoreCase));
                if (entry == null) continue;
                child.SetValue(entry.CloseDebit.ToString("F0"));
            }
        }

        var cdtkCoCuoiuKy = xmlDoc.GetChildElementByPath(root + "PLuc/PL_CDTK/SoDuCuoiKy/Co");
        if (cdtkCoCuoiuKy != null)
        {
            foreach (var child in cdtkCoCuoiuKy.Descendants())
            {
                var entry = report.TrialBalanceEntries
                                  .FirstOrDefault(x => ("ct" + x.AccountCode).Equals(
                                                      child.Name.LocalName,
                                                      StringComparison.InvariantCultureIgnoreCase));
                if (entry == null) continue;
                child.SetValue(entry.CloseCredit.ToString("F0"));
            }
        }

        return xmlDoc;
    }

    private async Task MapBalanceSheetAsync(FinancialReportWork report)
    {
        if (report.TrialBalanceEntries.Count == 0)
        {
            throw new NotFoundException("""
                                        Current report has no trial balance data, 
                                        please input trial balance data first.
                                        """);
        }

        //Reset to zero before adding new data to ensure consistency
        foreach (var b in report.BalanceSheetEntries)
        {
            b.EndingBalance = 0;
            b.BeginingBalance = 0;
        }

        //Find the balance sheet mappings for the regulation
        var balanceSheetMap = await balancesheetMapRepo.Find(m => m.RegulationId == report.Regulation
                                                                  && m.Account != null)
                                                       .Include(m => m.Account)
                                                       .ToListAsync();
        Console.WriteLine($"Found: {balanceSheetMap.Count} mapping.");

        foreach (var map in balanceSheetMap)
        {
            var trialEntry = report.TrialBalanceEntries
                                   .FirstOrDefault(x => x.AccountCode == map.Account!.Code);
            if (trialEntry == null) continue;

            var balanceEntry = report.BalanceSheetEntries
                                     .FirstOrDefault(x => x.BalanceSheetItemId == map.BalanceSheetItemId);
            if (balanceEntry is not null)
            {
                Console.WriteLine("Mapping account: <" + map.Account!.Code + "> to [" + balanceEntry.Code + "]");

                balanceEntry.BalanceSheetItemId = map.BalanceSheetItemId;
                balanceEntry.EndingBalance += map.ValueType switch
                {
                    ValueType.Credit => map.NegativeValue switch
                    {
                        true => trialEntry.CloseCredit * (-1),
                        _ => trialEntry.CloseCredit
                    },
                    ValueType.Debit => map.NegativeValue switch
                    {
                        true => trialEntry.CloseDebit * (-1),
                        _ => trialEntry.CloseDebit
                    },
                    _ => 0
                };
                balanceEntry.BeginingBalance += map.ValueType switch
                {
                    ValueType.Credit => map.NegativeValue switch
                    {
                        true => trialEntry.OpenCredit * (-1),
                        _ => trialEntry.OpenCredit
                    },
                    ValueType.Debit => map.NegativeValue switch
                    {
                        true => trialEntry.OpenDebit * (-1),
                        _ => trialEntry.OpenDebit
                    },
                    _ => 0
                };
            }
        }

        //calculate un-mapped item based on its child items
        foreach (var entry in report.BalanceSheetEntries)
        {
            var childOf = report.BalanceSheetEntries.Where(x => x.ParentCode == entry.Code).ToList();
            if (childOf.Count > 0)
            {
                entry.BeginingBalance = childOf.Sum(c => c.BeginingBalance);
                entry.EndingBalance = childOf.Sum(c => c.EndingBalance);
            }
        }
    }

    private async Task MapIncomeStatementAsync(FinancialReportWork report)
    {
        var maps = await incomeStatementMapRepo.Find(m => m.RegulationId == report.Regulation)
                                               .Include(m => m.IncomeStatementItem)
                                               .Include(m => m.Account)
                                               .ToListAsync();
        foreach (IncomeStatementMap map in maps)
        {
            var trialBalanceEntry = report.TrialBalanceEntries
                                          .FirstOrDefault(x => x.AccountCode == map.Account.Code);
            var incomeStatementEntry = report.IncomeStatementEntries
                                             .FirstOrDefault(x => x.IncomeStatementItemId ==
                                                                  map.IncomeStatementItem.Id);
            if (trialBalanceEntry is not null && incomeStatementEntry is not null)
            {
                incomeStatementEntry.ThisYear = trialBalanceEntry.AriseDebit;
            }
        }

        var itemHasChild = report.IncomeStatementEntries
                                 .Where(i => i.IncomeStatementItem.HasChild)
                                 .ToList();
        foreach (var item in itemHasChild)
        {
            item.ThisYear = report.IncomeStatementEntries
                                  .Where(i => i.IncomeStatementItem.ParentCode == item.Code)
                                  .Sum(i => i.ThisYear);
        }
    }

    private async Task<(string Filename, byte[] File)> CreateFinancialStatementNoteExcel(FinancialReportWork report)
    {
        var templateFilename = await dbContext.FinancialStatementNotes
                                              .FirstOrDefaultAsync(m => m.RegulationId == report.Regulation)
                               ?? throw new NotFoundException("Financial statement note not found");
        var templateExcel = LoadExcelTemplate(ExportTemplateFolder, templateFilename.TemplateFile);
        var sh = templateExcel.Worksheets[0];
        sh.Range["A1"].Value = report.Organization?.FullName;
        sh.Range["H3"].Value2 = report.Year;
        sh.Range["K3"].Value2 = report.ReportDate;
        var trialBalance = report.TrialBalanceEntries;
        sh.Range["J46"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "111")?.CloseDebit;
        sh.Range["K46"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "111")?.OpenDebit;
        sh.Range["J47"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "112")?.CloseDebit;
        sh.Range["K47"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "112")?.OpenDebit;
        // sh.Range["J48"].Value2 = trialBalance
        //                                .FirstOrDefault(x => x.AccountCode == "128")?.CloseDebit;
        // sh.Range["K48"].Value2 = trialBalance
        //                                .FirstOrDefault(x => x.AccountCode == "128")?.OpenDebit;

        sh.Range["J53"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "121")?.CloseDebit;
        sh.Range["K53"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "121")?.OpenDebit;

        sh.Range["J57"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "128")?.CloseDebit;
        sh.Range["K57"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "128")?.OpenDebit;

        sh.Range["J58"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1281")?.CloseDebit;
        sh.Range["K58"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1281")?.OpenDebit;
        sh.Range["J59"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1288")?.CloseDebit;
        sh.Range["K59"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1288")?.OpenDebit;

        sh.Range["J61"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2291")?.CloseDebit;
        sh.Range["K61"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2291")?.OpenDebit;
        sh.Range["J62"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2292")?.CloseDebit;
        sh.Range["K62"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2292")?.OpenDebit;

        sh.Range["J66"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "131")?.CloseDebit;
        sh.Range["K66"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "131")?.OpenDebit;

        sh.Range["J68"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "331")?.CloseDebit;
        sh.Range["K68"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "331")?.OpenDebit;

        sh.Range["J72"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "331")?.CloseDebit;
        sh.Range["K72"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "331")?.OpenDebit;

        sh.Range["J72"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "141")?.CloseDebit;
        sh.Range["K72"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "141")?.OpenDebit;

        sh.Range["J73"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1368")?.CloseDebit;
        sh.Range["K73"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1368")?.OpenDebit;

        sh.Range["J74"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1388")?.CloseDebit
                                 + trialBalance.FirstOrDefault(x => x.AccountCode == "338")?.CloseDebit;
        sh.Range["J74"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1388")?.OpenDebit
                                 + trialBalance.FirstOrDefault(x => x.AccountCode == "338")?.OpenDebit;

        sh.Range["J75"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1381")?.CloseDebit;
        sh.Range["J75"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "1381")?.OpenDebit;

        sh.Range["J84"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "151")?.CloseDebit;
        sh.Range["K84"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "151")?.OpenDebit;

        sh.Range["J85"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "152")?.CloseDebit;
        sh.Range["K85"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "152")?.OpenDebit;
        sh.Range["J86"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "153")?.CloseDebit;
        sh.Range["K86"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "153")?.OpenDebit;
        sh.Range["J87"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "154")?.CloseDebit;
        sh.Range["K87"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "154")?.OpenDebit;
        sh.Range["J88"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "155")?.CloseDebit;
        sh.Range["K88"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "155")?.OpenDebit;
        sh.Range["J89"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "156")?.CloseDebit;
        sh.Range["K89"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "156")?.OpenDebit;
        sh.Range["J90"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "157")?.CloseDebit;
        sh.Range["K90"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "157")?.OpenDebit;

        sh.Range["H96"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2111")?.OpenDebit;
        sh.Range["I96"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2111")?.AriseDebit;
        sh.Range["J96"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2111")?.AriseCredit;
        sh.Range["K96"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2111")?.CloseDebit;
        sh.Range["H97"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2141")?.OpenDebit;
        sh.Range["I97"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2141")?.AriseDebit;
        sh.Range["J97"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2141")?.AriseCredit;
        sh.Range["K97"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2141")?.CloseDebit;

        sh.Range["H100"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2113")?.OpenDebit;
        sh.Range["I100"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2113")?.AriseDebit;
        sh.Range["J100"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2113")?.AriseCredit;
        sh.Range["K100"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2113")?.CloseDebit;
        sh.Range["H101"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2143")?.OpenDebit;
        sh.Range["I101"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2143")?.AriseDebit;
        sh.Range["J101"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2143")?.AriseCredit;
        sh.Range["K101"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2143")?.CloseDebit;

        sh.Range["H104"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2112")?.OpenDebit;
        sh.Range["I104"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2112")?.AriseDebit;
        sh.Range["J104"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2112")?.AriseCredit;
        sh.Range["K104"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2112")?.CloseDebit;
        sh.Range["H105"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2142")?.OpenDebit;
        sh.Range["I105"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2142")?.AriseDebit;
        sh.Range["J105"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2142")?.AriseCredit;
        sh.Range["K105"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2142")?.CloseDebit;

        sh.Range["H112"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "217")?.OpenDebit;
        sh.Range["I112"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "217")?.AriseDebit;
        sh.Range["J112"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "217")?.AriseCredit;
        sh.Range["K112"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "217")?.CloseDebit;
        sh.Range["H113"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2147")?.OpenDebit;
        sh.Range["I113"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2147")?.AriseDebit;
        sh.Range["J113"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2147")?.AriseCredit;
        sh.Range["K113"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2147")?.CloseDebit;

        sh.Range["J122"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2411")?.CloseDebit;
        sh.Range["K122"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2411")?.OpenDebit;
        sh.Range["J123"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2412")?.CloseDebit;
        sh.Range["K123"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2412")?.OpenDebit;
        sh.Range["J124"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2413")?.CloseDebit;
        sh.Range["K124"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "2413")?.OpenDebit;

        sh.Range["J129"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "242")?.CloseDebit;
        sh.Range["K129"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "242")?.OpenDebit;
        sh.Range["J130"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "333")?.CloseDebit;
        sh.Range["K130"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "333")?.OpenDebit;

        sh.Range["J134"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "331")?.CloseCredit;
        sh.Range["K134"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "331")?.OpenCredit;
        sh.Range["J135"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "131")?.CloseCredit;
        sh.Range["K135"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "131")?.OpenCredit;
        sh.Range["J137"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "335")?.CloseCredit;
        sh.Range["K137"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "335")?.OpenCredit;
        sh.Range["J138"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "336")?.CloseCredit;
        sh.Range["K138"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "336")?.OpenCredit;
        sh.Range["J140"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3381")?.CloseCredit;
        sh.Range["K140"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3381")?.OpenCredit;

        var cacKhoanNopTheoLuong = trialBalance
                                   .Where(x => new List<string> { "3282", "3383", "3384", "3385" }.Contains(
                                              x.AccountCode))
                                   .ToList();
        sh.Range["J141"].Value2 = cacKhoanNopTheoLuong.Sum(x => x.CloseCredit);
        sh.Range["K141"].Value2 = cacKhoanNopTheoLuong.Sum(x => x.OpenCredit);
        sh.Range["J142"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3388")?.CloseCredit;
        sh.Range["L142"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3388")?.OpenCredit;

        sh.Range["H147"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3331")?.OpenCredit;
        sh.Range["I147"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3331")?.AriseCredit;
        sh.Range["J147"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3331")?.AriseDebit;
        sh.Range["K147"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3331")?.CloseCredit;

        sh.Range["H148"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3332")?.OpenCredit;
        sh.Range["I148"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3332")?.AriseCredit;
        sh.Range["J148"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3332")?.AriseDebit;
        sh.Range["K148"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3332")?.CloseCredit;

        sh.Range["H149"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3333")?.OpenCredit;
        sh.Range["I149"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3333")?.AriseCredit;
        sh.Range["J149"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3333")?.AriseDebit;
        sh.Range["K149"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3333")?.CloseCredit;

        sh.Range["H150"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3334")?.OpenCredit;
        sh.Range["I150"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3334")?.AriseCredit;
        sh.Range["J150"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3334")?.AriseDebit;
        sh.Range["K150"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3334")?.CloseCredit;

        sh.Range["H151"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3335")?.OpenCredit;
        sh.Range["I151"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3335")?.AriseCredit;
        sh.Range["J151"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3335")?.AriseDebit;
        sh.Range["K151"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3335")?.CloseCredit;

        sh.Range["H152"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3336")?.OpenCredit;
        sh.Range["I152"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3336")?.AriseCredit;
        sh.Range["J152"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3336")?.AriseDebit;
        sh.Range["K152"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3336")?.CloseCredit;

        sh.Range["H153"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3337")?.OpenCredit;
        sh.Range["I153"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3337")?.AriseCredit;
        sh.Range["J153"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3337")?.AriseDebit;
        sh.Range["K153"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3337")?.CloseCredit;

        sh.Range["H154"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33381")?.OpenCredit;
        sh.Range["I154"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33381")?.AriseCredit;
        sh.Range["J154"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33381")?.AriseDebit;
        sh.Range["K154"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33381")?.CloseCredit;

        sh.Range["H155"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33382")?.OpenCredit;
        sh.Range["I155"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33382")?.AriseCredit;
        sh.Range["J155"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33382")?.AriseDebit;
        sh.Range["K155"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "33382")?.CloseCredit;

        sh.Range["H156"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3339")?.OpenCredit;
        sh.Range["I156"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3339")?.AriseCredit;
        sh.Range["J156"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3339")?.AriseDebit;
        sh.Range["K156"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3339")?.CloseCredit;

        sh.Range["H161"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3411")?.OpenCredit;
        sh.Range["I161"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3411")?.AriseCredit;
        sh.Range["J161"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3411")?.AriseDebit;
        sh.Range["K161"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3411")?.CloseCredit;

        sh.Range["H163"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3412")?.OpenCredit;
        sh.Range["I163"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3412")?.AriseCredit;
        sh.Range["J163"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3412")?.AriseDebit;
        sh.Range["K163"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3412")?.CloseCredit;

        sh.Range["J168"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3521")?.CloseCredit;
        sh.Range["K168"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3521")?.OpenCredit;

        sh.Range["J169"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3522")?.CloseCredit;
        sh.Range["K169"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3522")?.OpenCredit;

        sh.Range["J170"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3524")?.CloseCredit;
        sh.Range["K170"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "3524")?.OpenCredit;

        //Capital:
        sh.Range["E177"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4111")?.OpenCredit;
        sh.Range["F177"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4112")?.OpenCredit;
        sh.Range["G177"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4118")?.OpenCredit;
        sh.Range["H177"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "419")?.OpenCredit;
        sh.Range["I177"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "413")?.OpenCredit;
        sh.Range["J177"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "421")?.OpenCredit
                                  + trialBalance.FirstOrDefault(x => x.AccountCode == "418")?.OpenCredit
                                  - trialBalance.FirstOrDefault(x => x.AccountCode == "421")?.OpenDebit;

        sh.Range["E178"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4111")?.AriseCredit;
        sh.Range["F178"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4112")?.AriseCredit;
        sh.Range["G178"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4118")?.AriseCredit;
        sh.Range["H178"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "419")?.AriseCredit;
        sh.Range["I178"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "413")?.AriseCredit;
        sh.Range["J178"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "421")?.AriseCredit
                                  + trialBalance.FirstOrDefault(x => x.AccountCode == "418")?.AriseCredit;

        sh.Range["E179"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4111")?.AriseDebit;
        sh.Range["F179"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4112")?.AriseDebit;
        sh.Range["G179"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "4118")?.AriseDebit;
        sh.Range["H179"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "419")?.AriseDebit;
        sh.Range["I179"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "413")?.AriseDebit;
        sh.Range["J179"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "421")?.AriseDebit
                                  + trialBalance.FirstOrDefault(x => x.AccountCode == "418")?.AriseDebit;

        //Income:
        sh.Range["J211"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "5111")?.AriseDebit;
        sh.Range["J212"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "5112")?.AriseDebit;
        sh.Range["J213"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "5113")?.AriseDebit;
        sh.Range["J214"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "5118")?.AriseDebit;

        //Cost:
        sh.Range["J229"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "632")?.AriseDebit;
        sh.Range["J245"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "635")?.AriseDebit;
        sh.Range["J255"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "6422")?.AriseDebit;
        sh.Range["J255"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "6421")?.AriseDebit;

        //Other income and expense:
        sh.Range["J267"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "711")?.AriseDebit;
        sh.Range["J274"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "811")?.AriseDebit;

        //Income Tax:
        sh.Range["J278"].Value2 = trialBalance.FirstOrDefault(x => x.AccountCode == "821")?.AriseDebit;

        using var stream = new MemoryStream();
        templateExcel.SaveToStream(stream, FileFormat.Version2016);
        stream.Position = 0;
        return (templateFilename.TemplateFile, stream.ToArray());
    }

    private Workbook LoadExcelTemplate(string templateFolder, string templateFile)
    {
        Workbook workbook = new()
        {
            Version = ExcelVersion.Version2016
        };
        string filePath = Path.Combine(env.ContentRootPath, templateFolder, templateFile);
        if (!File.Exists(filePath)) throw new NotFoundException("Template file not found.");
        workbook.LoadFromFile(filePath);
        return workbook;
    }

    #endregion
}