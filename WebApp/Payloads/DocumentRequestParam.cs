using WebApp.Enums;

namespace WebApp.Payloads;

public class DocumentRequestParam : RequestParam
{
    public List<DocumentType> DocumentTypes { get; set; } = [];
}
