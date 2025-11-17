using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Payloads;
using WebApp.Services.TaxDutyServices;
using WebApp.Services.TaxDutyServices.Dto;

namespace WebApp.Controllers;

[ApiController, Route("/api/tax-duty")] [Authorize]
public class TaxReportDutyController(ITaxDutyAppService service) : ControllerBase
{
    [HttpGet("get-all")]
    public async Task<IActionResult> Find([FromQuery] RequestParam input)
    {
        var result = await service.FindTaxDuties(input);
        return Ok(result);
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(TaxDutyDto input)
    {
        var result = await service.CreateTaxDuty(input);
        return Ok(result);
    }

    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, TaxDutyDto input)
    {
        var result = await service.UpdateTaxDutyById(id, input);
        return Ok(result);
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await service.SoftDeleteTaxDutyById(id);
        return Ok(result);
    }

    [HttpPost("add-duty")]
    public async Task<IActionResult> AddDutyToOrganization(OrganizationTaxDutiesDto input)
    {
        var result = await service.AddOrUpdateTaxDutyForOrganization(input);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("registered-duties/{organizationId:guid}")]
    public async Task<IActionResult> GetTaxDutiesByOrganizationId(Guid organizationId)
    {
        var result = await service.GetTaxDutiesByOrganizationId(organizationId);
        return Ok(result);
    }

    
}