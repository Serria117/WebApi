using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.Services.TaxProcedureService;
using WebApp.Services.TaxProcedureService.dto;

namespace WebApp.Controllers;
[ApiController, Authorize, Route("api/tax-procedure")]
public class TaxProcedureController(ITaxProcedureAppService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(TaxProcedureCreateDto dto)
    {
        var result = await service.CreateTaxProcedure(dto);
        return Ok(result);
    }

    [HttpPost("create-many")]
    public async Task<IActionResult> CreateMany([FromBody] ICollection<TaxProcedureCreateDto> dtos)
    {
        var result = await service.CreateManyTaxProcedures(dtos);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword)
    {
        var result = await service.GetAllTaxProcedures(keyword);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> FindById([FromRoute] int id)
    {
        var result = await service.FindTaxProcedureById(id);
        return Ok(result);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] TaxProcedureUpdateDto dto)
    {
        var result = await service.UpdateTaxProcedure(dto);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [HasAuthority(Permissions.TaxProcedureDelete)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await service.DeleteTaxProcedure(id);
        return Ok(result);
    }

    [HttpPost("delete-all")]
    [HasAuthority(Permissions.TaxProcedureDelete)]
    public async Task<IActionResult> DeleteMany([FromBody] int[] ids)
    {
        var result = await service.DeleteManyTaxProcedures(ids);
        return Ok(result);
    }
}
