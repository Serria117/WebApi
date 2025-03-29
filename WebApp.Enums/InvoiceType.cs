namespace WebApp.Enums;

public enum InvoiceType
{
    InvoiceWithCode = 5,
    InvoiceWithoutCode = 6,
    InvoiceFromPos = 8
}

public enum InvoiceStatus
{
    New = 1,
    Replacement = 2,
    Replaced = 3,
    Modifying = 4,
    Modified = 5,
    Terminated = 6,
    
}