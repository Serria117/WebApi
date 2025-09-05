using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.BalanceSheetService;

namespace WebApp.Controllers;

[ApiController, Authorize, Route("api/balance-sheet")]
public class BalanceSheetController(IBalanceSheetAppService service) : ControllerBase
{
    
}