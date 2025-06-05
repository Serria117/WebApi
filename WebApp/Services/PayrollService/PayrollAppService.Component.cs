using Microsoft.EntityFrameworkCore;
using WebApp.Core.DomainEntities.Salary;
using WebApp.Payloads;
using WebApp.Services.Mappers;
using WebApp.Services.PayrollService.Dto;
using WebApp.Utils;

namespace WebApp.Services.PayrollService;

public partial class PayrollAppService
{
    #region PAYROLL CONFIGURATION

    #region Payroll Component

    public async Task<AppResponse> CreatePayrollComponentCategory(PayrollComponentCategoryCreateDto input)
    {
        try
        {
            var category = new PayrollComponentCategory
            {
                Name = input.Name,
                Description = input.Description,
            };

            await transactionManager.BeginAsync();
            var result = await PayrollComponentCategoryRepository.CreateAsync(category, true);
            await transactionManager.CommitAsync();
            return AppResponse.OkResult(result);
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

    public async Task<AppResponse> GetPayrollComponentCategories()
    {
        try
        {
            var result = await PayrollComponentCategoryRepository.Find(x => !x.Deleted)
                                                                 .ToListAsync();
            return AppResponse.OkResult(result);
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> GetPayrollComponentCategoryById(int id)
    {
        try
        {
            var result = await PayrollComponentCategoryRepository.Find(x => x.Id == id && !x.Deleted)
                                                                 .FirstOrDefaultAsync();
            return result == null
                ? AppResponse.Error404("Payroll component category not found")
                : AppResponse.OkResult(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> CreatePayrollComponentType(PayrollComponentTypeCreateDto input)
    {
        try
        {
            var component = input.ToEntity();
            var result = await PayrollComponentTypeRepository.CreateAsync(component);
            return AppResponse.OkResult(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> GetPayrollComponentTypes()
    {
        try
        {
            var result = await PayrollComponentTypeRepository.Find(x => !x.Deleted)
                                                             .ToListAsync();
            return AppResponse.OkResult(result);
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    public async Task<AppResponse> GetPayrollComponentTypeById(int id)
    {
        try
        {
            var result = await PayrollComponentTypeRepository.Find(x => x.Id == id && !x.Deleted)
                                                             .FirstOrDefaultAsync();
            return result == null
                ? AppResponse.Error404("Payroll component type not found")
                : AppResponse.OkResult(result.ToDisplayDto());
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    #endregion

    #region Payroll Input

    public async Task<AppResponse> CreatePayrollInputType(PayrollInputTypeCreateDto input)
    {
        try
        {
            var item = input.ToEntity();
            var result = await PayrollInputTypeRepository.CreateAsync(item);
            return AppResponse.OkResult(result);
        }
        catch (Exception e)
        {
            log.LogErrorFormatted(exception: e);
            return AppResponse.Error(e.Message);
        }
    }

    #endregion

    #endregion
}