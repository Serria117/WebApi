using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.InvoiceService;

namespace WebApp.Controllers;

[ApiController]
[Route("/api/invoice-history")]
[Authorize]
public class InvoiceHistoryController(IInvoiceHistoryAppService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHistory([FromQuery] RequestParam pr)
    {
        var req = PageRequest.FromParams(pr);
        var response = await service.GetHistoryAsync(req);
        return response.Success ? Ok(response) : BadRequest(response.Message);
    }
}
