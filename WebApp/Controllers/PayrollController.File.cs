using Microsoft.AspNetCore.Mvc;
using WebApp.Enums;
using WebApp.Services.PayrollService.Dto;

namespace WebApp.Controllers;

public partial class PayrollController
{
    /// <summary>
    /// Processes and uploads payroll data from an Excel file.
    /// </summary>
    /// <param name="input">An object containing the payroll data extracted from the uploaded Excel file. Cannot be null.</param>
    /// <returns>An IActionResult that represents the result of the upload operation. Returns a success response with the upload
    /// result if the operation completes successfully.</returns>
    [HttpPost("file/upload")]
    public async Task<IActionResult> UploadExcelPayroll(ExcelPayrollDto input)
    {
        var result = await service.UploadExcelPayroll(input);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a list of payroll Excel files filtered by year and keyword.
    /// </summary>
    /// <param name="year">The payroll year to filter by. Specify a year to return files for that year; or null to include all years.</param>
    /// <param name="keyword">An optional keyword to filter the payroll files. Only files containing this keyword in their metadata will be
    /// included. Specify null to include all files.</param>
    /// <returns>An <see cref="IActionResult"/> containing the filtered list of payroll Excel files. The result is returned as a
    /// JSON array. If no files match the criteria, the array will be empty.</returns>
    [HttpGet("file")]
    public async Task<IActionResult> FindExcelPayroll(int? year, string? keyword)
    {
        var result = await service.GetPayrollExcelList(year, keyword);
        return Ok(result);
    }

    /// <summary>
    /// Downloads a specific payroll Excel file by its unique identifier.
    /// </summary>
    /// <param name="id">The file's identifier to be retrieved</param>
    /// <returns>The byte array of the file, and the filename</returns>
    [HttpGet("file/download/{id}")]
    public async Task<IActionResult> DownloadExcelPayroll(string id)
    {
        var (fileName, file) = await service.DownloadExcelPayroll(id);
        Response.Headers.Append("X-Filename", $"{fileName}");
        return File(file, ContentType.ApplicationOfficeSpreadSheet, fileName);
    }

    /// <summary>
    /// Deletes the Excel payroll file with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the Excel payroll file to delete. Cannot be null or empty.</param>
    /// <returns>An IActionResult indicating the result of the delete operation. Returns a success response if the file was
    /// deleted; otherwise, returns an error response.</returns>
    [HttpDelete("file/{id}")]
    public async Task<IActionResult> DeleteExcelPayroll(string id)
    {
        var result = await service.DeleteExcelPayroll(id);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing Excel payroll file with the specified data.
    /// </summary>
    /// <param name="input">An object containing the updated payroll information to apply to the Excel file. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the update operation. Returns a success response with
    /// the update result if the operation completes successfully.</returns>
    [HttpPut("file/update")]
    public async Task<IActionResult> UpdateExcelPayrollFile(ExcelPayrollUpdateDto input)
    {
        var result = await service.UpdateExcelPayroll(input);
        return Ok(result);
    }

    /// <summary>
    /// Updates the status of a payroll Excel file identified by the specified ID.
    /// </summary>
    /// <param name="id">The unique identifier of the payroll Excel file whose status is to be updated. Cannot be null.</param>
    /// <returns>An <see cref="IActionResult"/> that represents the result of the update operation.</returns>
    [HttpPut("file/update-status/{id}")]
    public async Task<IActionResult> UpdatePayrollExcelStatus(string id)
    {
        var result = await service.UpdateExcelPayrollStatus(id);
        return Ok(result);
    } 
}
