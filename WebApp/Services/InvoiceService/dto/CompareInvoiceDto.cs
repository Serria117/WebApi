namespace WebApp.Services.InvoiceService.dto;

/// <summary>
/// This class represent extracted invoice record for comparision with existing data in the database
/// </summary>
public class CompareInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string InvoiceNotation { get; set; } = string.Empty;
    public DateTime? InvoiceDate { get; set; }
    public string SellerTaxCode { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public decimal GoodPrice { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalPrice { get; set; }
    public string InvoiceStatus { get; set; } = string.Empty;

    public bool NotFound { get; set; } = false;
    public decimal TotalPriceDiff { get; set; } = 0;
}