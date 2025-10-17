using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services.TemplateServices;
using WebApp.Enums;
using WebApp.Services.TemplateServices.Dto;
using WebApp.Payloads;

namespace WebApp.Controllers;
[ApiController, Route("api/template"), Authorize]
public class TemplateController(ITemplateAppService service,
                                ILogger<TemplateController> logger) : ControllerBase
{
    /// <summary>
    /// Download template file
    /// </summary>
    /// <param name="fileId">The identifier of the template file</param>
    /// <returns>The template file</returns>
    [HttpGet("download/{fileId:int}")]
    public async Task<IActionResult> DownloadTemplate(int fileId)
    {
        try
        {
            (string fileName, byte[] fileData) = await service.DownloadFile(fileId);
            Response.Headers["X-Filename"] = fileName;
            return File(fileData, ContentType.ApplicationOctetStream, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Retrieve the collection of templates
    /// </summary>
    /// <param name="parameters">Query parameters to specify conditions to filter the template collection</param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> GetTemplates([FromQuery] RequestParam parameters)
    {
        var req = PageRequest.FromParams(parameters);
        var result = await service.FindTemplates(req);
        return Ok(result);
    }

    /// <summary>
    /// Retrieve a single template by its identifier
    /// </summary>
    /// <param name="id">The template identifier to be retrieved</param>
    /// <returns></returns>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTemplateById(int id)
    {
        var result = await service.GetTemplateById(id);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create new template
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateTemplate(TemplateCreateDto input)
    {
        var res = await service.CreateTemplate(input);
        return Ok(res);
    }

    /// <summary>
    /// Upload a template file to a specific template
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadTemplateFile(TemplateFileUploadDto input)
    {
        var res = await service.UploadNewFileToTemplate(input);
        return res.Code switch
        {
            "200" => Ok(res),
            "404" => NotFound(res),
            "400" => BadRequest(res),
            _ => StatusCode(500, res)
        };
    }

    /// <summary>
    /// Delete a template file
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteTemplateFile(int id)
    {
        try
        {
            await service.DeleteTemplateFile(id);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting template file with id {TemplateId}", id);
            return BadRequest("Invalid template Id");
        }
    }
}