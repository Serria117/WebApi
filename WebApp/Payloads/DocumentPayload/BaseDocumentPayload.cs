using WebApp.Enums;

namespace WebApp.Payloads.DocumentPayload;

// Base class to extract document's details from the xml file.
public class BaseDocumentPayload
{
    public string OrganizationName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DocumentType DocumentType { get; set; }
    public DateTime DocumentDate { get; set; }
    public string? DocumentName { get; set; }
    public int? Period { get; set; }
    public int? Year { get; set; }
    public string? AdjustmentType { get; set; }
    public string? PeriodType { get; set; }
    public int NumberOfAdjustment { get; set; } = 0;
}