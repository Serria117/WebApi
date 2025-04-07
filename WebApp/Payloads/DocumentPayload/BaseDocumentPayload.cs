using WebApp.Enums;

namespace WebApp.Payloads.DocumentPayload;

// Base class to extract document's details from the xml file.
public class BaseDocumentPayload
{
    public string OrganizationName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public DateTime IssueDate { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
}