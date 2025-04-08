using WebApp.Enums;

namespace WebApp.Payloads.DocumentPayload;

// Base class to extract document's details from the xml file.
public class BaseDocumentPayload
{
    public string OrganizationName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DocumentType DocumentType { get; set; }
    public DateTime IssueDate { get; set; }
    public string? DocumentName { get; set; }
    public string? Period { get; set; }
}