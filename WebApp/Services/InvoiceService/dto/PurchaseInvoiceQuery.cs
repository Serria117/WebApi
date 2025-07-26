namespace WebApp.Services.InvoiceService.dto;

public class PurchaseInvoiceQuery
{
    public string? BuyerTaxId { get; set; }
    public string? SellerTaxId { get; set; }
    public string? InvoiceCode { get; set; }
    public string? InvoiceNumber { get; set; }
}