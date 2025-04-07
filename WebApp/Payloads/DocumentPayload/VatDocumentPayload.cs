namespace WebApp.Payloads.DocumentPayload;

public class VatDocumentPayload : BaseDocumentPayload
{
    public long Ct21 { get; set; }
    public long Ct22 { get; set; }
    public long Ct23 { get; set; }
}