using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Services.AccountingServices;
using WebApp.Services.AccountingServices.Dto;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Utils;

namespace WebApp.Controllers;

[ApiController, Route("api/accounting"), Authorize]
public class AccountingController(IFinancialStatementAppService service,
                                  ILogger<AccountingController> logger) : ControllerBase
{
    [HttpGet("regulation")]
    public async Task<IActionResult> GetRegulations()
    {
        var result = await service.GetRegulationList();
        return Ok(result);
    }

    /// <summary>
    /// Get a list of accounts associated with the selected accounting regulation
    /// </summary>
    /// <param name="regulationId">Regulation's identifier related to the accounts</param>
    /// <returns></returns>
    [HttpGet("account/{regulationId:int}")]
    public async Task<IActionResult> GetAccountTemplate([FromRoute] int regulationId)
    {
        var result = await service.GetAccountList(regulationId);
        return Ok(result);
    }

    /// <summary>
    /// Get a list of Financial Report of the current working organization
    /// </summary>
    /// <param name="fromYear"></param>
    /// <param name="toYear"></param>
    /// <returns></returns>
    [HttpGet("financial-report/get-all")]
    [HasAuthority(Permissions.FinancialReportView)]
    public async Task<IActionResult> GetFinancialReportWorkList([FromQuery] int? fromYear,
                                                                [FromQuery] int? toYear)
    {
        var result = await service.GetFinancialReportList(fromYear, toYear);
        return result.Code switch
        {
            "404" => NotFound(),
            "500" => StatusCode(500, result),
            "400" => BadRequest(result),
            "200" => Ok(result),
            _ => StatusCode(500, result)
        };
    }

    /// <summary>
    /// Get a list of Financial Report of the last year
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    [HttpGet("financial-report/last-year-report/{year:int}")]
    public async Task<IActionResult> GetLastYearList(int year)
    {
        var result = await service.GetLastYearReports(year);
        return Ok(result);
    }

    /// <summary>
    /// Assign a report from the previous year to the current financial report.
    /// </summary>
    /// <param name="req">The request body contain the current report Id and the last year report Id</param>
    /// <returns>A response indicating the success or failure of the operation.</returns>
    [HttpPut("financial-report/assign-last-year-report")]
    public async Task<IActionResult> AssignLastYearReport(AssignLastYearReportRequest req)
    {
        var result = await service.SelectLastYearReport(req.ReportId, req.LastYearReportId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single Report by its identifier
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("financial-report/{id}")]
    [HasAuthority(Permissions.FinancialReportView)]
    public async Task<IActionResult> GetFinancialReportById(string id)
    {
        var result = await service.GetFinancialReportById(id);
        return Ok(result);
    }

    /// <summary>
    /// Initiate new Financial Report
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("financial-report/create")]
    [HasAuthority(Permissions.FinancialReportCreate)]
    public async Task<IActionResult> CreateFinancialReport(FinancialReportWorkDto input)
    {
        try
        {
            await service.CreateFinancialReportWork(input);
            return Ok("Created successfully.");
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Import user's trial balance from an Excel template file.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("financial-report/import")]
    [HasAuthority(Permissions.FinancialReportCreate)]
    public async Task<IActionResult> ImportUserInputData(UserInputExcelFile input)
    {
        try
        {
            var result = await service.ImportUserInputTrialBalanceFromExcelFile(input);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest();
        }
    }

    /// <summary>
    /// Update the user's trial balance input
    /// </summary>
    /// <param name="id"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("financial-report/update-input/{id}")]
    [HasAuthority(Permissions.FinancialReportUpdate)]
    public async Task<IActionResult> UpdateUserInputTrialBalance([FromRoute] string id,
                                                                 List<UserBalanceEntryUpdate> input)
    {
        try
        {
            var result = await service.UpdateUserInputTrialBalance(id, input);
            return Ok(result);
        }
        catch (NotFoundException e)
        {
            logger.LogErrorFormatted(exception: e);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest();
        }
    }

    /// <summary>
    /// Clear all user input data for a specific financial report.
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpDelete("financial-report/clear-all-user-input/{reportId}")]
    [HasAuthority(Permissions.FinancialReportDelete)]
    public async Task<IActionResult> ClearUserInputData(string reportId)
    {
        try
        {
            await service.ClearAllUserInputTrialEntries(reportId);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest();
        }
    }

    /// <summary>
    /// Remove specific user input data entries by their IDs.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpDelete("financial-report/delete-user-input")]
    [HasAuthority(Permissions.FinancialReportDelete)]
    public async Task<IActionResult> RemoveUserInputDataById(long[] ids)
    {
        var result = await service.DeleteUserInputTrialBalance(ids);
        return Ok(result);
    }

    /// <summary>
    /// Completely remove the financial report and all it related data from database.
    /// </summary>
    /// <param name="id">Identifier of the report to be removed.</param>
    /// <returns></returns>
    [HttpDelete("financial-report/delete-report/{id}")]
    [HasAuthority(Permissions.FinancialReportDelete)]
    public async Task<IActionResult> HardDeleteReport(string id)
    {
        var result = await service.HardDeleteReport(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Map data from user input trial balance to main trial balance
    /// </summary>
    /// <param name="reportId">Identifier of the report to perform mapping</param>
    /// <returns></returns>
    [HttpPut("financial-report/map-trial-balance/{reportId}")]
    public async Task<IActionResult> MapTrialBalanceFromInput(string reportId)
    {
        try
        {
            var result = await service.MapUserTrialBalance(reportId);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Calculate report
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpPut("financial-report/calc/{reportId}")]
    public async Task<IActionResult> CalculateFinancialStatement(string reportId)
    {
        try
        {
            var result = await service.CalculateFinancialStatement(reportId);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Reset all report value
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpPut("financial-report/reset/{reportId}")]
    public async Task<IActionResult> ResetFinancialStatement(string reportId)
    {
        try
        {
            var result = await service.ResetReport(reportId);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Perform mapping for income statement
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpPut("financial-report/income-statement/map/{reportId}")]
    public async Task<IActionResult> MapIncomeStatement(string reportId)
    {
        try
        {
            var result = await service.MapIncomeStatementFromTrialBalance(reportId);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Reset income statement data
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpPut("financial-report/income-statement/reset/{reportId}")]
    public async Task<IActionResult> ResetIncomeStatement(string reportId)
    {
        try
        {
            var result = await service.ResetIncomeStatement(reportId);
            return Ok(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Create the XML document of the financial report
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpPut("financial-report/xml-doc/create/{reportId}")]
    public async Task<IActionResult> CreateXmlDocument(string reportId)
    {
        var result = await service.CreateOrUpdateXmlDocument(reportId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Download the financial report XML data that compartible with HTKK 5.x program
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpGet("financial-report/xml-doc/download/{reportId}")]
    public async Task<IActionResult> DownloadXmlDocument([FromRoute] string reportId)
    {
        var result = await service.DownloadXmlDocument(reportId);
        Response.Headers.Append("X-Filename", result.FileName);
        return File(result.File, ContentType.ApplicationXml, result.FileName);
    }

    /// <summary>
    /// Download the financial report note in Excel format
    /// </summary>
    /// <param name="reportId"></param>
    /// <returns></returns>
    [HttpGet("financial-report/note/download/{reportId}")]
    public async Task<IActionResult> DownloadFinancialReportNote([FromRoute] string reportId)
    {
        try
        {
            var result = await service.ExportReportNoteExcel(reportId);
            Response.Headers.Append("X-Filename", result.FileName);
            return File(result.File, ContentType.ApplicationOfficeSpreadSheet, result.FileName);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("financial-report/template/trial-balance")]
    public async Task<IActionResult> DownloadTrialBalanceTempate()
    {
        try
        {
            var result = await service.DownloadTrialBalanceTemplate();
            Response.Headers.Append("X-Filename", result.FileName);
            return File(result.File, ContentType.ApplicationOfficeSpreadSheet, result.FileName);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("financial-report/import-xml/{id}")]
    public async Task<IActionResult> ImportFinancialReportFromXml([FromRoute] string id,
                                                                  IFormFile xmlFile)
    {
        try
        {
            var result = await service.ImportFinancialReportFromXml(id, xmlFile);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception e)
        {
            logger.LogErrorFormatted(exception: e);
            return BadRequest(e.Message);
        }
    }
}