namespace WebApp.Services.InvoiceService.dto;

public class SoldInvoiceErrorDisplay
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string? BuyerTaxId { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PurchaseInvoiceErrorDisplay
{
    public string? InvoiceNumber { get; set; } = string.Empty;
    public string? SellerName { get; set; } = string.Empty;
    public string? SellerTaxId { get; set; } = string.Empty;
    public DateTime? IssueDate { get; set; }
    public string? ErrorMessage { get; set; }
}