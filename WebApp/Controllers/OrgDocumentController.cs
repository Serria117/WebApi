using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Payloads;
using WebApp.Services.DocumentService;

namespace WebApp.Controllers;

[ApiController, Route("api/document")]
[Authorize]
public class OrgDocumentController(IDocumentAppService documentService, 
                                   IHostEnvironment env) : ControllerBase
{
    /// <summary>
    /// Upload documents
    /// </summary>
    /// <param name="orgId">The organization Id</param>
    /// <param name="file">The list of file to be uploaded</param>
    /// <returns></returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(List<IFormFile> file)
    {
        var response = await documentService.UploadDocFileAsync(file);
        return Ok(response);
    }

    /// <summary>
    /// Get uploaded files of an organization
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="documentType"></param>
    /// <param name="reqParam"></param>
    /// <returns></returns>
    [HttpGet("get-document/{orgId:guid}")]
    public async Task<IActionResult> GetFilesByOrgAndType(Guid orgId,
                                                          [FromQuery] DocumentType documentType,
                                                          [FromQuery] RequestParam reqParam)
    {
        var response = await documentService.GetFilesByOrgAndTypeAsync(documentType, reqParam);
        return Ok(response);
    }

    /// <summary>
    /// Download a specific document
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="documentId"></param>
    /// <returns></returns>
    [HttpGet("download/{orgId:guid}")]
    public async Task<IActionResult> DownloadDocument(Guid orgId, [FromQuery] int documentId)
    {
        var fileResponse = await documentService.GetFileByIdAsync(documentId);
        if (!fileResponse.Success || fileResponse.Data is null)
        {
            return NotFound("File not found or you don't have permission to access the file");
        }
        
        string filePath = Path.Combine(env.ContentRootPath, (string) fileResponse.Data);
        
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("The file may have been moved or deleted");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        const string contentType = "application/xml";
        var fileName = Path.GetFileName(filePath);
        return File(fileStream, contentType, fileName);
    }

    [HttpGet("read/01gtgt")]
    public async Task<IActionResult> Read_01GTGT(int docId)
    {
        var res = await documentService.Read_01GTGT_Xml(docId);
        if (res.Success) return Ok(res);
        return NotFound(res);
    }

    /// <summary>
    /// Deletes a document by its ID.
    /// </summary>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <returns>An IActionResult indicating the result of the delete operation.</returns>
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteDocument([FromBody] int documentId)
    {
        var response = await documentService.DeleteFileByIdAsync(documentId);

        if (response.Success)
        {
            return Ok(response);
        }

        return int.TryParse(response.Code, out int statusCode) 
            ? StatusCode(statusCode, response) 
            : StatusCode(500, "An unexpected error occurred."); // Default to 500 if parsing fails
    }

}