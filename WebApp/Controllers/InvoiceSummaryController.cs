using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.InvoiceService;
using WebApp.Services.OrganizationService;

namespace WebApp.Controllers;

[ApiController, Route("/api/invoice-summary"), Authorize]
public class InvoiceSummaryController(IInvoiceService invoiceService) : ControllerBase
{
    /// <summary>
    /// Retrieves summarized information about sellers' invoices.
    /// </summary>
    /// <returns>
    /// An IActionResult containing the summary of sellers' invoices wrapped in a ResponseEntity object.
    /// </returns>
    [HttpGet("seller")]
    public async Task<IActionResult> GetSellersSummarize([FromQuery] int? year = null,
                                                         [FromQuery] string? keyword = null)
    {
        return Ok(await invoiceService.ScanOrganizationSeller(year, keyword));
    }

    /// <summary>
    /// Retrieves summarized information about buyers' invoices.
    /// </summary>
    /// <returns>
    /// An IActionResult containing the summary of buyers' invoices wrapped in a ResponseEntity object.
    /// </returns>
    [HttpGet("buyer")]
    public async Task<IActionResult> GetBuyersSummarize([FromQuery] int? year = null,
                                                        [FromQuery] string? keyword = null)
    {
        return Ok(await invoiceService.ScanOrganizationBuyer(year, keyword));
    }
}