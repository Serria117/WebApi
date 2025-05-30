﻿using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.InvoiceService;
using WebApp.Services.InvoiceService.dto;
using WebApp.Services.RestService;
using WebApp.Services.RestService.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/invoice")]
[Authorize]
public class InvoiceController(IRestAppService restService,
                               IInvoiceAppService invService, 
                               ISoldInvoiceAppService soldInvoiceService,
                               ILogger<InvoiceController> logger) : ControllerBase
{
    /// <summary>
    /// Get capcha data from hoadondientu.gdt.gov.vn for login
    /// </summary>
    /// <returns></returns>
    [HttpGet("get-captcha")]
    public async Task<IActionResult> GetCaptcha()
    {
        var result = await restService.GetCaptcha();
        return Ok(result);
    }

    /// <summary>
    /// Authenticate with hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="loginModel"></param>
    /// <returns></returns>
    [HttpPost("invoice-login")]
    public async Task<IActionResult> LoginInvoiceService(InvoiceLoginModel loginModel)
    {
        var result = await restService.Authenticate(loginModel);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Find the purchase invoices in database based on query parameters
    /// </summary>
    /// <param name="taxId">The taxId of the buyer</param>
    /// <param name="parameters">The query params object</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("find/{taxId}")]
    public async Task<IActionResult> FindPurchaseInvoices(string taxId, 
                                                          [FromQuery] InvoiceRequestParam parameters,
                                                          CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await invService.FindPurchaseInvoices(taxId, parameters.Valid());
            return Ok(result);
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning("Request was canceled. {message}", e.Message);
            return StatusCode(499, "Request canceled by the client or server.");
        }
    }

    /// <summary>
    /// Sync invoices with detail from hoadondientu.gdt.gov.vn
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("sync")]
    public async Task<IActionResult> SyncInvoice(SyncInvoiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await invService.ExtractPurchaseInvoices(request.Token, request.From, request.To);
            return Ok(res);
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning("Request was canceled. {message}", e.Message);
            return StatusCode(499, "Request canceled by the client or server.");
        }
    }

    /// <summary>
    /// Download invoices list as Excel file
    /// </summary>
    /// <param name="taxId"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="cancellationToken">The cancellation token used to propagate notification that operation should be canceled</param>
    /// <returns>The Excel file contains invoices in search range</returns>
    [HttpGet("download/{taxId}")]
    public async Task<IActionResult> DownloadInvoiceSummaryExcelFile(string taxId, string from, string to, 
                                                                     CancellationToken cancellationToken)
    {
        try
        {
            var fileByte = await invService.ExportExcel(taxId, from, to);
            if (fileByte is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    code = StatusCodes.Status500InternalServerError.ToString(),
                    message = "Failed to export excel file. Check log for more information."
                });
            }

            var fileName = $"{taxId}_{from}_{to}_{Ulid.NewUlid()}.xlsx";
            Response.Headers["X-Filename"] = fileName;
            return File(fileByte, ContentType.ApplicationOfficeSpreadSheet, fileName);
        }
        catch (OperationCanceledException e) // Operation was cancelled.
        {
            logger.LogWarning("Request was canceled. {message}", e.Message);
            return StatusCode(499, "Request canceled by the client or server.");
        }
    }

    /// <summary>
    /// Recheck if invoices status has changed
    /// </summary>
    /// <param name="request"></param>
    /// <returns>The number of invoice that has been updated</returns>
    [HttpPost("recheck")]
    public async Task<IActionResult> RecheckInvoice(SyncInvoiceRequest request)
    {
        var result = await invService.RecheckPurchaseInvoice(request.Token, request.From, request.To);
        return result.Code == "207"
            ? StatusCode(StatusCodes.Status207MultiStatus, result)
            : Ok(result);
    }

    /// <summary>
    /// Get a single invoice value
    /// </summary>
    /// <param name="taxId">Organization's taxId</param>
    /// <param name="id">Invoice id</param>
    /// <returns></returns>
    [HttpGet("get/{taxId}")]
    public async Task<IActionResult> GetInvoice(string taxId, string id)
    {
        var result = await invService.FindOne(taxId, id);
        return result.Code == "200" ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Retrieve sold invoices from hoadondientu service
    /// </summary>
    /// <param name="request">Request parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of successfully added invoice (duplicate value will be ignored)</returns>
    [HttpPost("sold-invoice/sync")]
    public async Task<IActionResult> RetrieveSoldInvoice(SyncInvoiceRequest request, 
                                                         CancellationToken cancellationToken)
    {
        try
        {
            //var result = await invService.ExtractSoldInvoice(request);
            var result = await soldInvoiceService.GetInvoiceFromService(request.Token, request.From, request.To);
            return Ok(result);
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning("Request was canceled. {message}", e.Message);
            return StatusCode(499, "Request canceled by the client or server.");
        }
    }

    /// <summary>
    /// Find sold invoice in the database
    /// </summary>
    /// <param name="taxId">Organization taxid</param>
    /// <param name="parameters">Query parameters</param>
    /// <returns>List of sold invoice</returns>
    [HttpGet("sold/find/{taxId}")][HasAuthority(Permissions.InvoiceQuery)]
    public async Task<IActionResult> FindSoldInvoices(string taxId, [FromQuery] InvoiceRequestParam parameters)
    {
        try
        {
            var result = await invService.FindSoldInvoices(taxId, parameters);
            return Ok(result);
        }
        catch (OperationCanceledException e)
        {
            logger.LogWarning("Request was canceled. {message}", e.Message);
            return StatusCode(499, "Request canceled by the client or server.");
        }
    }
}