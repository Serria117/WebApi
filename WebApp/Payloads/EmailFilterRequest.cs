using System.Diagnostics.CodeAnalysis;

namespace WebApp.Payloads;

public class EmailFilterRequest
{
    public string Email { get; set; } = string.Empty;
    public string? Sender { get; set; }
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public string? FileName { get; set; }
    public string?  FileType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}