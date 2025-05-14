namespace WebApp.Enums;

/// <summary>
/// Represents a collection of constant hub names used for notifications
/// in the application. This structure is typically used for defining
/// consistent string values for referencing specific notification hubs.
/// </summary>
public struct HubName
{
    public const string InvoiceMessage = "InvoiceMessage";
    public const string InvoiceStatus = "InvoiceStatus";
}