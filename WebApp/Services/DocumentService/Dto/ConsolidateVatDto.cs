using WebApp.Payloads.DocumentPayload;

namespace WebApp.Services.DocumentService.Dto;

/// <summary>
/// Represents a data transfer object for consolidating VAT-related information.
/// </summary>
public class ConsolidateVatDto
{
    public string KyThue { get; set; } = string.Empty;
    public string LoaiTk { get; set; } = string.Empty;
    public int LanNop { get; set; } = 0;
    public DateTime NgayTk { get; set; }
    public Document01GtgtPayload? Payload {get;set;}
    public string? Url { get; set; }
}