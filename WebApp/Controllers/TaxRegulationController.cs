using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.TaxRegulationServices;
using WebApp.Services.TaxRegulationServices.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/tax-regulation"), Authorize]
public class TaxRegulationController(ITaxRegulationAppService taxRegulationService) : ControllerBase
{
    [HttpPost("pit/")]
    public async Task<IActionResult> CreatePITRegulation([FromBody] PersonalIncomeRegulationDto input)
    {
        await taxRegulationService.CreatePITRegulation(input);
        return Ok();
    }

    [HttpGet("pit/")]
    public async Task<IActionResult> FindPITRegulations([FromQuery] string? keyword)
    {
        var result = await taxRegulationService.FindPITRegulations(keyword);
        return Ok(result);
    }

    [HttpGet("pit/{id:guid}")]
    public async Task<IActionResult> GetPITBracketById(Guid id)
    {
        var result = await taxRegulationService.GetPITRegulationById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
