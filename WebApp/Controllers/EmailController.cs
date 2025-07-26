using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.EmailService;

namespace WebApp.Controllers;
[ApiController, Route("/api/email")]
[Authorize]
public class EmailController(IEmailAppService emailService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> FindEmail([FromQuery] EmailFilterRequest request)
    {
        var emails = await emailService.FindEmailsAsync(request);
        return Ok(emails);
    }
}
