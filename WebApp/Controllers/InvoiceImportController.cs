using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.InvoiceService;
using WebApp.Services.InvoiceService.dto;

namespace WebApp.Controllers;
[ApiController, Route("api/invoice-import"), Authorize]
public class InvoiceImportController(IInvoiceImportService invoiceImportService) : ControllerBase
{
    [HttpPost("purchase")]
    public async Task<IActionResult> ImportPurchaseInvoices(List<IFormFile> files)
    {
        var result = await invoiceImportService.ImportPurchaseInvoice(files);
        return Ok(result);
    }
}