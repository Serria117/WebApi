using Microsoft.AspNetCore.Mvc;
using WebApp.Core.DomainEntities;
using WebApp.Services.TaxDutyServices;
using WebApp.Services.TaxDutyServices.Dto;

namespace WebApp.Controllers;

[ApiController, Route("api/tax-duty-records")]
public class TaxReportDutyRecordController(ITaxDutyRecordAppService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> FindDutyRecords([FromQuery] int? year,
                                                     [FromQuery] DutyPeriodType? periodType,
                                                     [FromQuery] string? period,
                                                     [FromQuery] DutyStatus? status,
                                                     [FromQuery] Guid? organizationId,
                                                     [FromQuery] string? keyword)
    {
        var result = await service.FindTaxDutyRecords(year: year,
                                                      periodType: periodType,
                                                      period: period,
                                                      status: status,
                                                      orgId: organizationId,
                                                      keyword: keyword);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTaxDutyRecord([FromBody] TaxDutyRecordsCreateDto input)
    {
        var result = await service.CreateDutyRecordsForOrganizations(input);
        return Ok(result);
    }
    
}