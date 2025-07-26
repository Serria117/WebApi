namespace WebApp.Services.RestService.Dto;

public class InvoiceAuthenticationResponse
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public string Token { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? Timestamp { get; set; }
    public string? Path { get; set; }
}
