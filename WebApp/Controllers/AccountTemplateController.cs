using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.BalanceSheetService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("api/accounting")]
[Authorize]
public class AccountTemplateController(IBalanceSheetAppService balanceSheetService,
                                       ILogger<AccountTemplateController> logger)
            : ControllerBase
{
    [HttpGet("account/{regulationId:int}")]
    public async Task<IActionResult> GetAccountTemplate([FromRoute] int regulationId)
    {
        var result = await balanceSheetService.GetAccountList(regulationId);
        return Ok(result);
    }

    [HttpGet("financial-report/get-all")]
    public async Task<IActionResult> GetFinancialReportWork()
    {
        var result = await balanceSheetService.GetFinancialReportList();
        return Ok(result);
    }

    [HttpPost("financial-report/create")]
    public async Task<IActionResult> CreateFinancialReport(FinancialReportWorkDto input)
    {
        try
        {
            await balanceSheetService.CreateFinancialReportWork(input);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("financial-report/user-input")]
    public async Task<IActionResult> AddUserInputBalancesheet(UserInputBalancesheetDto input)
    {
        try
        {
            var result = await balanceSheetService.CreateUserInputBalancesheet(input);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e.StackTrace);
            return BadRequest();
        }
    }
}