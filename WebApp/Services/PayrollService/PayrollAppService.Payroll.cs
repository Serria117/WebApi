using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebApp.Core.DomainEntities.Salary;
using WebApp.Enums;
using WebApp.Enums.Payroll;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Payloads.Payroll;
using WebApp.Services.CommonService;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;
using X.Extensions.PagedList.EF;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    #region PAYROLL

    #region Payroll Period

    public async Task<AppResponse> InitPayrollByYearAsync(InitPayrollPeriodRequest request)
    {
        try
        {
            await transactionManager.BeginAsync();
            var org = await OrganizationRepository.Find(x => x.Id == WorkingOrg.ToGuid() && !x.Deleted)
                                                  .FirstOrDefaultAsync();
            if (org is null) return AppResponse.Error400("Invalid organization ID");

            // Find the maximum version for the given year
            var lastVersion = await PayrollPeriodRepository.Find(x => x.OrganizationId == org.Id
                                                                      && x.Year == request.Year
                                                                      && !x.Deleted)
                                                           .MaxAsync(x => (int?)x.Version);

            var version = (lastVersion ?? 0) + 1;

            var payrollsInYear = new List<PayrollPeriod>();
            for (int i = 1; i <= 12; i++)
            {
                var startDate = new DateTime(request.Year, i, 1);
                var endDate = new DateTime(request.Year, i, DateTime.DaysInMonth(request.Year, i));
                var netWorkDays = request.NetWorkDays ?? GetNetWorkDays(startDate, endDate, request.WeekendType);
                var period = new PayrollPeriodCreateDto
                {
                    Year = request.Year,
                    Month = i,
                    StartDate = startDate,
                    EndDate = endDate,
                    NetWorkDays = netWorkDays,
                    Version = version,
                    Name = $"{i}/{request.Year} - V.{version:00}",
                }.ToEntity(org);
                payrollsInYear.Add(period);
            }

            await PayrollPeriodRepository.CreateManyAsync(payrollsInYear, inTransaction: true);

            // Find all employees that are eligible for payroll in the given year
            var validEmployees = await EmployeeRepository
                                       .Find(x => !x.Deleted
                                                  && x.OrganizationId == org.Id
                                                  && x.HireDate <= payrollsInYear.First().EndDate
                                                  && (x.TerminationDate == null ||
                                                      x.TerminationDate >=
                                                      payrollsInYear.Last().EndDate))
                                       .ToListAsync();

            // If no valid employees found, return error
            if (validEmployees.IsNullOrEmpty())
                return AppResponse.Error400("No valid employees found for payroll period");

            // Create payroll records for each employee in each period
            foreach (PayrollPeriod period in payrollsInYear)
            {
                var employeesInPeriod = validEmployees
                                        .Where(x => !x.Deleted
                                                    && x.HireDate <= period.EndDate
                                                    && (x.TerminationDate == null ||
                                                        x.TerminationDate >= period.EndDate))
                                        .ToList();
                if (employeesInPeriod.IsNullOrEmpty()) continue;
                var records = await CreatePayrollRecordsForEmployeesAsync(period, employeesInPeriod);
                period.PayrollRecords = records.ToHashSet();
            }

            // Create timesheets for each payroll record
            foreach (var period in payrollsInYear.Where(period => !period.PayrollRecords.IsNullOrEmpty()))
            {
                await CreateTimesheetsAsync(period);
            }

            await transactionManager
                .CommitAsync(); // Commit the transaction after all payroll periods and records are created
            return AppResponse.Ok();
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            // Rollback the transaction in case of any error
            await transactionManager.RollbackAsync();
            return AppResponse.Error500(ResponseMessage.GenericError);
        }
        finally
        {
            // Ensure the transaction manager is disposed
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> CreatePayrollPeriodAsync(PayrollPeriodCreateDto input)
    {
        try
        {
            var org = await OrganizationRepository.Find(x => x.Id == WorkingOrg.ToGuid() && !x.Deleted)
                                                  .FirstOrDefaultAsync();
            if (org == null) return AppResponse.Error400("Invalid organization ID");

            var period = input.ToEntity(org);

            await transactionManager.BeginAsync();
            var result = await PayrollPeriodRepository.CreateAsync(period);
            await transactionManager.CommitAsync();

            return AppResponse.OkResult(result);
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(ResponseMessage.GenericError);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> GetPayrollPeriods(PayrollPeriodRequest request)
    {
        try
        {
            var query = PayrollPeriodRepository
                .Find(x => !x.Deleted && x.OrganizationId == WorkingOrg.ToGuid());

            if (request.Year is not null)
            {
                query = query.Where(x => x.Year == request.Year);
                if (request.Month is not null)
                {
                    query = query.Where(x => x.Month == request.Month);
                }

                if (request.Version is not null)
                {
                    query = query.Where(x => x.Version == request.Version);
                }
            }

            var result = await query.Include(x => x.PayrollRecords)
                                    .OrderByDescending(x => x.Year).ThenBy(x => x.Version)
                                    .ToListAsync();

            var groupedResult = result.GroupBy(x => x.Year)
                                      .ToDictionary(
                                          g => g.Key,
                                          g => g.Select(x => x.ToDisplayDto()).ToList()
                                      );
            var annualResult = groupedResult.Keys.Select(resultKey => new AnnualPayrollPeriodResponse
            {
                Year = resultKey,
                PayrollPeriods = groupedResult[resultKey]
            }).ToList();
            return AppResponse.OkResult(annualResult);
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }


    public async Task<AppResponse> GetPayrollPeriodById(int id)
    {
        try
        {
            var result = await PayrollPeriodRepository
                               .Find(x => x.Id == id
                                          && !x.Deleted
                                          && x.OrganizationId == WorkingOrg.ToGuid())
                               .FirstOrDefaultAsync();
            return result == null
                ? AppResponse.Error404("Payroll period not found")
                : AppResponse.OkResult(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    #endregion

    #region Payroll Record

    public async Task<AppResponse> CreatePayrollRecordsAsync(PayrollRecordCreateDto input)
    {
        try
        {
            if (input.EmployeeId.IsNullOrEmpty()) return AppResponse.Error400("Employee ID is required");

            await transactionManager.BeginAsync();
            var period = await PayrollPeriodRepository.Find(x => x.Id == input.PeriodId
                                                                 && !x.Deleted
                                                                 && x.OrganizationId == WorkingOrg.ToGuid())
                                                      .FirstOrDefaultAsync();
            if (period == null)
                return AppResponse.Error400("Invalid payroll period ID");

            //Find all employees that match the given IDs and are eligible for the period
            var employees = await EmployeeRepository
                                  .Find(x => input.EmployeeId.Contains(x.Id)
                                             && !x.Deleted
                                             && x.HireDate <= period.EndDate
                                             && (x.TerminationDate == null || x.TerminationDate >= period.EndDate)
                                             && x.OrganizationId == WorkingOrg.ToGuid())
                                  .ToListAsync();
            if (employees.IsNullOrEmpty())
                return AppResponse.Error400("Invalid employee ID or employee not eligible for this period");

            List<PayrollRecord> records = [];
            records.AddRange(employees.Select(employee => input.ToEntity(employee, period)));


            await PayrollRecordRepository.CreateManyAsync(records, inTransaction: true);
            await transactionManager.CommitAsync();

            return AppResponse.Ok();
        }
        catch (Exception e)
        {
            await transactionManager.RollbackAsync();
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
        finally
        {
            await transactionManager.DisposeAsync();
        }
    }

    public async Task<AppResponse> GetPayrollRecordsInPeriodAsync(int periodId)
    {
        try
        {
            var result = await PayrollRecordRepository.Find(x => !x.Deleted && periodId == x.Id)
                                                      .Include(x => x.Employee)
                                                      .Include(x => x.PayrollPeriod)
                                                      .ToListAsync();
            return AppResponse.OkResult(result);
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> GetPayrollRecordByIdAsync(long id)
    {
        try
        {
            var result = await PayrollRecordRepository
                               .Find(x => x.Id == id && !x.Deleted)
                               .Include(x => x.Employee)
                               .Include(x => x.PayrollPeriod)
                               .Include(x => x.PayrollItems)
                               .FirstOrDefaultAsync();


            return result == null
                ? AppResponse.Error404("Payroll record not found")
                : AppResponse.OkResult(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    #endregion

    #endregion

    /// <summary>
    /// Calculates the number of net work days between two dates, considering weekends.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="weekendType"></param>
    /// <returns>The total net work days within date range</returns>
    private static decimal GetNetWorkDays(DateTime startDate, DateTime endDate,
                                          WeekendType weekendType = WeekendType.Sunday)
    {
        decimal weekendDays = 0;
        var totalDays = (decimal)(endDate - startDate).TotalDays + 1; // Include both start and end dates
        var sundayCount = 0;
        var saturdayCount = 0;

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    sundayCount++;
                    break;
                case DayOfWeek.Saturday:
                    saturdayCount++;
                    break;
            }
        }

        switch (weekendType)
        {
            case WeekendType.Sunday:
                weekendDays += sundayCount;
                break;
            case WeekendType.SaturdayAndSunday:
                weekendDays += saturdayCount + sundayCount;
                break;
            case WeekendType.HalfSaturdayAndSunday:
                weekendDays += (saturdayCount * 0.5m) + sundayCount;
                break;
            case WeekendType.Undefined:
                break;
        }

        return totalDays - weekendDays;
    }

    private async Task<List<PayrollRecord>> CreatePayrollRecordsForEmployeesAsync(
        PayrollPeriod period, List<Employee> employees)
    {
        List<PayrollRecord> records = [];
        if (employees.IsNullOrEmpty()) return records;

        records.AddRange(employees.Select(employee => new PayrollRecord
        {
            Employee = employee,
            PayrollPeriod = period,
        }));

        await PayrollRecordRepository.CreateManyAsync(records, inTransaction: true);
        return records;
    }

    private async Task<PayrollCalculationResult> CalculatePayrollItemsAsync(PayrollRecord record)
    {
        if (record is null) throw new NotFoundException("Payroll record not found");

        //Calculate actual work days:
        var timesheets = await TimesheetRepository.Find(x => x.PayrollRecordId == record.Id && !x.Deleted)
                                                  .ToListAsync();
        if (timesheets.IsNullOrEmpty())
            throw new NotFoundException("No timesheets found for the given payroll record");
        record.ActualWorkDays = timesheets.Count(x => x is { IsWeekend: false, IsHoliday: false, IsTripDay: false });
        var lastDateInPeriod = timesheets.Select(t => t.Date).Max();
        //Calculate base salary of actual work days:
        var baseSalaryOfActualWorkDays =
            (record.Employee.BaseSalary ?? 0) * record.ActualWorkDays / record.PayrollPeriod.NetWorkDays;

        // TODO: Add additional payroll items calculation logic here
        decimal taxableAllowance = 0;
        decimal nontTaxableAllowance = 0;

        
        var allowanceRates = await GetAllowanceRateAsync(lastDateInPeriod);
        if (allowanceRates.IsNullOrEmpty())
        {
            taxableAllowance = allowanceRates.Where(a => a.IsTaxable)
                                             .Sum(a => a.Amount); // Taxable allowances
            nontTaxableAllowance = allowanceRates.Where(a => !a.IsTaxable)
                                                 .Sum(a => a.Amount); // Nontaxable allowances
        }

        //TODO: Add logic to calculate nontaxable income based on employee's allowances, benefits, etc.
        decimal nontaxableIncome = 0;
        nontaxableIncome += nontTaxableAllowance;

        var taxableIncome = baseSalaryOfActualWorkDays + taxableAllowance - nontaxableIncome;

        //TODO: Add logic to calculate excluded taxable income based on employee's insurrance and dependents.
        decimal excludedTaxableIncome = 0;

        int countDependent = 0;
        DependentDeductionAmount? dependentRate = null;
        decimal insuranceDeduction = 0;
        InsuranceRateGroup? insuranceGroup = null;
        decimal selfDeduction = 0;
        if (record.TaxType == TaxType.ResidentProgressive)
        {
            //Get self deduction amount for the employee
            selfDeduction = (await GetSelfDeductionAmountAsync(lastDateInPeriod))?.Amount ?? 0;

            excludedTaxableIncome += selfDeduction;

            //Count employee dependents that effectively active in the period:
            countDependent = record.Employee.Dependents
                                   .Count(d => d.EndDate >= lastDateInPeriod || d.EndDate == null);

            dependentRate = await GetDependentRateAsync(timesheets.Max()!.Date);
            var deductionForDependent = countDependent * dependentRate.Amount;

            excludedTaxableIncome += deductionForDependent;

            // Calculate insurance deductions based on the employee's insurance salary
            insuranceGroup = await GetInsuranceRateGroupAsync(lastDateInPeriod);
            if (insuranceGroup is not null)
            {
                insuranceDeduction = insuranceGroup.InsuranceRates
                                                   .Where(r => r.IsEmployeePaid)
                                                   .Sum(insuranceRate =>
                                                            (record.Employee.InsuranceSalary ?? 0) *
                                                            insuranceRate.Rate);
            }

            excludedTaxableIncome += insuranceDeduction;
        }

        var calculatedTaxableIncome = taxableIncome - excludedTaxableIncome;
        // Calculate income tax based on the taxable income and the employee's tax type
        var incomeTax = await CalculateIncomeTaxAsync(calculatedTaxableIncome, lastDateInPeriod, record.TaxType);

        return new PayrollCalculationResult
        {
            TaxType = record.TaxType,
            ActualWorkDays = record.ActualWorkDays,
            BaseSalaryOfActualWorkDays = baseSalaryOfActualWorkDays,
            NontaxableIncome = nontaxableIncome,
            SelfDeduction = selfDeduction,
            CountDependent = countDependent,
            DependentRate = dependentRate?.Amount ?? 0,
            Insurance = insuranceGroup?.InsuranceRates
                                      .Where(r => r.IsEmployeePaid)
                                      .Select(r => new InsuranceDetail
                                      {
                                          Name = r.Name,
                                          Rate = r.Rate
                                      }).ToList(),
            InsuranceDeduction = insuranceDeduction,
            CalculatedTaxableIncome = calculatedTaxableIncome,
            IncomeTax = incomeTax,
        };
    }

    /// <summary>
    /// Calculates the income tax based on the taxable income, date, and tax type.
    /// </summary>
    /// <param name="calculatedTaxableIncome">The taxable income to calculate tax for.</param>
    /// <param name="time">The date to determine applicable tax brackets or rates.</param>
    /// <param name="taxType">The type of tax to apply (e.g., NonResident, ResidentNonContract, ResidentProgressive).</param>
    /// <returns>The calculated income tax as a decimal value.</returns>
    /// <remarks>
    /// - For NonResident, a flat 20% tax rate is applied.
    /// - For ResidentNonContract, a flat 10% tax rate is applied.
    /// - For ResidentProgressive, progressive tax brackets are applied based on the date.
    /// </remarks>
    private async Task<decimal> CalculateIncomeTaxAsync(decimal calculatedTaxableIncome,
                                                        DateTime time,
                                                        TaxType taxType)
    {
        if (calculatedTaxableIncome <= 0)
            return 0; // No tax for zero or negative income
        switch (taxType)
        {
            case TaxType.NonResident:
                return Math.Round(calculatedTaxableIncome * 0.2m, 0);
            case TaxType.ResidentNonContract:
                return Math.Round(calculatedTaxableIncome * 0.1m, 0);
            case TaxType.ResidentProgressive:
            default:
                {
                    // For progressive tax, we need to apply the tax brackets
                    decimal totalTax = 0;
                    decimal remainingIncome = calculatedTaxableIncome;
                    var taxBrackets = await IncomeTaxBracketRepository
                                            .Find(x => !x.Deleted
                                                       && ((x.EffectiveDate <= time && x.EndDate >= time) ||
                                                           (x.EffectiveDate <= time && x.EndDate == null)))
                                            .OrderBy(x => x.Order)
                                            .ToListAsync();
                    foreach (IncomeTaxBracket taxBracket in taxBrackets)
                    {
                        decimal bracketUpperLimit = taxBracket.Max ?? calculatedTaxableIncome;

                        decimal incomeInCurrentBracket = Math.Min(remainingIncome, bracketUpperLimit - taxBracket.Min);
                        if (incomeInCurrentBracket > 0)
                        {
                            totalTax += incomeInCurrentBracket * taxBracket.TaxRate;
                            remainingIncome -= incomeInCurrentBracket;
                        }

                        if (remainingIncome <= 0)
                            break; // No more income to calculate tax for
                    }

                    return Math.Round(totalTax, 0);
                }
        }
    }

    /// <summary>
    /// Retrieves the dependent rate that is effective for the given date.
    /// </summary>
    /// <param name="time">The date for which the dependent rate is to be retrieved.</param>
    /// <returns>The dependent rate effective for the given date.</returns>
    /// <exception cref="NotFoundException">Thrown if no dependent rate is found for the given date.</exception>
    private async Task<DependentDeductionAmount> GetDependentRateAsync(DateTime time)
    {
        var found = await DependentRateRepository.Find(x => !x.Deleted &&
                                                            ((x.EffectiveDate <= time && x.EndDate >= time)
                                                             || (x.EffectiveDate <= time && x.EndDate == null)))
                                                 .OrderBy(x => x.EffectiveDate)
                                                 .FirstOrDefaultAsync();
        if (found == null)
            throw new NotFoundException("No dependent rate found for the given time");
        return found;
    }

    /// <summary>
    /// Retrieves the insurance rate group that is effective for the given date.
    /// </summary>
    /// <param name="time">The date for which the insurance rate group is to be retrieved.</param>
    /// <returns>The insurance rate group effective for the given date.</returns>
    /// <exception cref="NotFoundException">Thrown if no insurance rate group is found for the given date.</exception>
    private async Task<InsuranceRateGroup?> GetInsuranceRateGroupAsync(DateTime time)
    {
        var found = await InsuranceRateGroupRepository.Find(x => !x.Deleted &&
                                                                 ((x.EffectiveDate <= time && x.EndDate >= time)
                                                                  || (x.EffectiveDate <= time && x.EndDate == null)))
                                                      .OrderBy(x => x.EffectiveDate)
                                                      .FirstOrDefaultAsync();
        return found;
    }

    /// <summary>
    /// Retrieves the self-deduction amount that is effective for the given date.
    /// </summary>
    /// <param name="time">The date for which the self-deduction amount is to be retrieved.</param>
    /// <returns>The self-deduction amount effective for the given date.</returns>
    /// <exception cref="NotFoundException">Thrown if no self-deduction value is found for the given date.</exception>
    private async Task<SelfDeductionAmount?> GetSelfDeductionAmountAsync(DateTime time)
    {
        var found = await SelfDeductionAmountRepository.Find(x => !x.Deleted &&
                                                                  ((x.EffectiveDate <= time && x.EndDate >= time)
                                                                   || (x.EffectiveDate <= time && x.EndDate == null)))
                                                       .OrderBy(x => x.EffectiveDate)
                                                       .FirstOrDefaultAsync();
        return found;
    }

    private async Task<List<AllowanceRate>> GetAllowanceRateAsync(DateTime time)
    {
        // Retrieves the allowance rate of the current organization or a global allowance rate if no specific organization rate exists.
        var found = await AllowanceRateRepository.Find(a => !a.Deleted &&
                                                            (a.OrganizationId == WorkingOrg.ToGuid() ||
                                                             a.OrganizationId == null) &&
                                                            ((a.EffectiveDate <= time && a.EndDate >= time) ||
                                                             (a.EffectiveDate <= time && a.EndDate == null)))
                                                 .OrderBy(a => a.EffectiveDate)
                                                 .ToListAsync();
        return found;
    }


    /// <summary>
    /// Creates a new payroll input record based on the provided input data.
    /// </summary>
    /// <param name="input">The data transfer object containing the details of the payroll input to be created.</param>
    /// <returns>The created payroll input entity.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the input, input type, or payroll record is null.
    /// </exception>
    private async Task<PayrollInput> CreatePayrollInputAsync(PayrollInputCreateDto input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var inputType = await PayrollInputTypeRepository.Find(x => x.Id == input.PayrollInputTypeId && !x.Deleted)
                                                        .FirstOrDefaultAsync();

        if (inputType is null)
            throw new NotFoundException("No payroll input type found for the given time");

        var payrollRecord = await PayrollRecordRepository.Find(x => x.Id == input.PayrollRecordId && !x.Deleted)
                                                         .Include(x => x.Employee)
                                                         .FirstOrDefaultAsync();

        if (payrollRecord is null)
            throw new NotFoundException("No payroll record found for the given time");

        var payrollInput = input.ToEntity(inputType, payrollRecord);
        return await PayrollInputRepository.CreateAsync(payrollInput, inTransaction: true);
    }
}