using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.TaxRegulationServices;
using WebApp.Services.TaxRegulationServices.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/tax-regulation"), Authorize]
public class TaxRegulationController(ITaxRegulationAppService taxRegulationService,
                                     ISocialSecurityService socialSecurityService) : ControllerBase
{
    [HttpPost("pit")]
    public async Task<IActionResult> CreatePITRegulation([FromBody] PersonalIncomeRegulationDto input)
    {
        await taxRegulationService.CreatePITRegulation(input);
        return Ok();
    }

    [HttpGet("pit")]
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

    [HttpPost("social-security")]
    public async Task<IActionResult> CreateSocialSecurityRegulation([FromBody] SocialSecurityDto input)
    {
        var result = await socialSecurityService.CreateSocialSecurityRegulation(input);
        return Ok(result);
    }

    [HttpGet("social-security")]
    public async Task<IActionResult> FindSocialSecurityRegulations([FromQuery] string? keyword)
    {
        var result = await socialSecurityService.FindSocialSecurityRegulations(keyword);
        return Ok(result);
    }

    [HttpGet("social-security/{id}")]
    public async Task<IActionResult> GetSocialSecurityById([FromRoute] Guid id)
    {
        var result = await socialSecurityService.GetSocialSecurityRegulationById(id);
        return Ok(result);
    }

    [HttpPut("social-security/{id}")]
    public async Task<IActionResult> UpdateSocialSecurityRegulation([FromRoute] Guid id,
                                                                    [FromBody] SocialSecurityDto input)
    {
        var result = await socialSecurityService.UpdateSocialSecurityRegulation(id, input);
        return Ok(result);
    }

    [HttpDelete("social-security/{id}")]
    public async Task<IActionResult> DeleteSocialSecurityRegulation([FromRoute] Guid id)
    {
        var result = await socialSecurityService.SoftDelete(id);
        return Ok(result);
    }

    [HttpGet("pit/calculate")]
    public async Task<IActionResult> CalculatePIT([FromQuery] decimal monthlyIncome, 
                                                  [FromQuery]Guid regulationId)
    {
        var result = await taxRegulationService.CalculateMonthlyPIT(monthlyIncome, regulationId);
        return Ok(result);
    }
}
