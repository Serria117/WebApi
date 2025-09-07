using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Authentication;
using WebApp.Enums;
using WebApp.GlobalExceptionHandler.CustomExceptions;
using WebApp.Payloads;
using WebApp.Services.DocumentService;

namespace WebApp.Controllers;

[ApiController, Route("api/document")]
[Authorize]
public class OrgDocumentController(IDocumentAppService documentService,
                                   IHostEnvironment env,
                                   IConfiguration config) : ControllerBase
{
    private readonly string[] _allowedFileExt =
        config.GetSection("AllowedFileExt").Get<string[]>()
        ??
        [
            ".xml", ".pdf", ".xlsx", ".docx", ".jpg", ".jpeg", ".png", ".gif",
            ".txt", ".csv", ".zip", ".rar", ".7z", "json"
        ];

    /// <summary>
    /// Upload documents
    /// </summary>
    /// <param name="file">The list of file to be uploaded</param>
    /// <returns></returns>
    [HttpPost(Api.Upload)]
    public async Task<IActionResult> UploadDocument(List<IFormFile> file)
    {
        var allowedExt = file.Select(f => Path.GetExtension(f.FileName).ToLowerInvariant())
                             .All(ext => _allowedFileExt.Contains(ext, StringComparer.OrdinalIgnoreCase));
        if (!allowedExt)
        {
            return BadRequest(
                $"Only files with following extensions are allowed: {string.Join(", ", _allowedFileExt)}. Check your file(s) and try again.");
        }

        var response = await documentService.UploadDocFileAsync(file);
        return Ok(response);
    }

    /// <summary>
    /// Get uploaded files of an organization
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [HttpGet("get-document")]
    [HasAuthority(Permissions.DocumentView)]
    public async Task<IActionResult> GetDocumentsList([FromQuery] DocumentRequestParam req)
    {
        var response = await documentService.FindDocumentsAsync(req);
        return Ok(response);
    }

    /// <summary>
    /// Download a specific document
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    [HttpGet(Api.Download)]
    [HasAuthority(Permissions.DocumentView)]
    public async Task<IActionResult> DownloadDocument([FromQuery] int documentId)
    {
        var fileResponse = await documentService.GetDocumentByIdAsync(documentId);
        if (!fileResponse.Success || fileResponse.Data is null)
        {
            return NotFound("File not found or you don't have permission to access the file");
        }

        string filePath = Path.Combine(env.ContentRootPath, (string)fileResponse.Data);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("The file may have been moved or deleted");
        }

        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        const string contentType = "application/xml";
        var fileName = Path.GetFileName(filePath);
        return File(fileStream, contentType, fileName);
    }

    [HttpGet("export/{documentId:int}")]
    public async Task<IActionResult> ExportExcelDocument(int documentId)
    {
        try
        {
            var result = await documentService.ExportDocumentToExcel(documentId);
            if(result.Data is null) return NotFound("Document not found or unsupported document type");
            Response.Headers.Append("X-Filename", result.FileName);
            Console.WriteLine("File name: " + result.FileName);
            return File(result.Data, ContentType.ApplicationOfficeSpreadSheet, result.FileName);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Read a specific document from file
    /// </summary>
    /// <param name="docId">Document id</param>
    /// <returns></returns>
    [HttpGet("read")]
    public async Task<IActionResult> ReadDocument([FromQuery] int docId)
    {
        var res = await documentService.ReadDocumentFromFileAsync(docId);
        if (res.Success) return Ok(res);
        return NotFound(res);
    }

    /// <summary>
    /// Deletes a document by its ID.
    /// </summary>
    /// <param name="documentId">The ID of the document to delete.</param>
    /// <returns>An IActionResult indicating the result of the delete operation.</returns>
    [HttpDelete("delete/{documentId:int}")]
    public async Task<IActionResult> DeleteDocument([FromRoute] int documentId)
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

    /// <summary>
    /// Summarize document of 01/GTGT and download the result as Excel file.
    /// </summary>
    /// <param name="ids">Array of IDs for documents to consolidate.</param>
    /// <returns>The Excel file containing result</returns>
    [HttpPost("summarize/01gtgt")]
    public async Task<IActionResult> Summarize_01GTKT(List<int> ids)
    {
        try
        {
            var fileResult = await documentService.Summarize_01Gtkt_Documents(ids);
            Response.Headers.Append("X-Filename", fileResult.FileName);
            return File(fileResult.File, ContentType.ApplicationOfficeSpreadSheet, fileResult.FileName);
        }
        catch (EmptyResultException e)
        {
            return Ok(e.Message);
        }
        catch (Exception e)
        {
            return StatusCode(500, e.Message);
        }
    }

    [HttpPost("summarize/bctc133")]
    public async Task<IActionResult> Summarize_BCTC133(List<int> ids)
    {
        try
        {
            var fileResult = await documentService.Summarize_Bctc133_Document(ids);
            Response.Headers.Append("X-Filename", fileResult.FileName);
            return File(fileResult.File, ContentType.ApplicationOfficeSpreadSheet, fileResult.FileName);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (EmptyResultException e)
        {
            return Ok(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    /// <summary>
    /// Summarize document of 01/GTGT and download the result as Excel file.
    /// </summary>
    /// <param name="ids"></param>
    /// <returns></returns>
    [HttpPost("summarize/bctc200")]
    public async Task<IActionResult> Summarize_BCTC200(List<int> ids)
    {
        try
        {
            var fileResult = await documentService.Summarize_Bctc200_Document(ids);
            Response.Headers.Append("X-Filename", fileResult.FileName);
            return File(fileResult.File, ContentType.ApplicationOfficeSpreadSheet, fileResult.FileName);
        }
        catch (NotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (EmptyResultException e)
        {
            return Ok(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return StatusCode(500, e.Message);
        }
    }
    
    /// <summary>
    /// Check if the document is duplicated by comparing its hash value with existing hashes in database.
    /// </summary>
    /// <param name="hash">Md5hash value of the document</param>
    /// <returns>True if it's duplicate. False otherwise.</returns>
    [HttpPost("check-duplicate/{hash}")]
    public async Task<IActionResult> CheckDuplicate([FromRoute] string hash)
    {
        var response = await documentService.CheckHashForDuplicated(hash);
        return Ok(response);
    }

    [HttpPost("check-hash")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> TestHashing(IFormFile file)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0; // Reset position back to start
        var hashBytes = await md5.ComputeHashAsync(stream);
        var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        return Ok(hashString);
    }

    [HttpGet("read-xml-content")]
    public async Task<IActionResult> ReadXmlString([FromQuery] int documentId)
    {
        try
        {
            var res = await documentService.ReadXmlToStringAsync(documentId);
            return Ok(res);
        }
        catch (NotFoundException e)
        {
            return StatusCode(404, e.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal server error.");
        }
    }
}