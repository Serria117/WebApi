using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.AccountingServices;
using WebApp.Services.BalanceSheetService;
using WebApp.Services.BalanceSheetService.Dto;
using WebApp.Utils;

namespace WebApp.Controllers;

[ApiController, Authorize, Route("api/financial-statements")]
public class FinancialStatementController(IFinancialStatementAppService service,
                                          ILogger<FinancialStatementController> logger) : ControllerBase
{

}