namespace WebApp.Services.InvoiceService.dto;

public class PurchaseInvoiceQuery
{
    public string? BuyerTaxId { get; set; }
    public string? SellerTaxId { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? InvoiceNotation { get; set; }
    public int? InvoiceGroupNotation { get; set; }
}