using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApp.Controllers;
[ApiController, Route("/api/work-diary")][Authorize]
public class WorkDiaryController : ControllerBase
{
    
}