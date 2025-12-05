using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Core.DomainEntities;
using WebApp.Core.DomainEntities.Accounting.TaxDeclarations;
using WebApp.Enums;
using WebApp.Services.TaxDutyServices;
using WebApp.Services.TaxDutyServices.Dto;

namespace WebApp.Controllers;

[ApiController, Route("api/tax-duty-records")]
[Authorize]
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

    [HttpPut]
    public async Task<IActionResult> UpdateTaxDutyRecords(ICollection<TaxDutyRecordUpdateDto> input)
    {
        var result = await service.UpdateTaxDutyRecords(input);
        return Ok(result);
    }

    [HttpGet("templates")]
    public async Task<IActionResult> GetDocumentTemplate()
    {
        var result = await service.GetDocumentTemplates();
        return Ok(result);
    }

    [HttpGet("xml-list/by-taxId/{taxId}")]
    public async Task<IActionResult> GetDocumentByTaxId([FromRoute] string taxId,
                                                        [FromQuery] int fromYear, [FromQuery] int toYear,
                                                        [FromQuery] string[] templateCode,
                                                        [FromQuery] int page = 1, [FromQuery] int pageSize = 1000)
    {
        var result = await service.GetDocumentByTaxId(taxId, fromYear, toYear, templateCode, page,
                                                      pageSize);
        return Ok(result);
    }

    [HttpPost("xml-upload")]
    [HasAuthority(Permissions.DocumentUpload)]
    public async Task<IActionResult> UploadXmlForTaxDuty([FromForm] TaxDutyRecordUploadXml input)
    {
        var result = await service.UploadXmlToExistingRecord(input);
        if (result.Code == "409")
        {
            return Conflict(result);
        }

        return Ok(result);
    }

    [HttpPost("xml-multi-upload")]
    [HasAuthority(Permissions.DocumentUpload)]
    public async Task<IActionResult> UploadMultiFile(TaxDutyRecordUploadMultiXml input)
    {
        var result = await service.UploadMultiXmlFiles(input);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("xml-replace/{docId}")]
    [HasAllAuthorities(Permissions.DocumentUpload, Permissions.DocumentUpdate)]
    public async Task<IActionResult> ReplaceXmlDocument(string docId, [FromForm] TaxDutyRecordUploadXml input)
    {
        var result = await service.ReplaceXmlInExistingRecord(docId, input);
        return Ok(result);
    }

    [HttpPut("xml-status")]
    [HasAuthority(Permissions.DocumentUpdate)]
    public async Task<IActionResult> UpdateXmlStatus(TaxDutyDocumentUpdateStatus input)
    {
        var result = await service.UpdateXmlStatus(input);

        return Ok(result);
    }

    [HttpGet("xml-list/{id}")]
    [HasAuthority(Permissions.DocumentView)]
    public async Task<IActionResult> GetDocumentsByRecordId(string id)
    {
        return Ok(await service.GetDocumentsByRecord(id));
    }

    [HttpGet("xml-file/{docId}")]
    [HasAuthority(Permissions.DocumentView)]
    public async Task<IActionResult> GetXmlFile(string docId)
    {
        var result = await service.GetXmlContent(docId);
        Response.Headers.Append("X-Filename", result.FileName);
        return File(result.File, ContentType.ApplicationOfficeSpreadSheet, result.FileName);
    }

    [HttpDelete("xml-delete")]
    [HasAuthority(Permissions.DocumentDelete)]
    public async Task<IActionResult> RemoveXmlDocument([FromBody] TaxDutyDocumentRemoveDto input)
        => Ok(await service.RemoveXmlDocument(input.DocumentId, input.Permanent));
}