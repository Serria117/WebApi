namespace WebApp.Payloads.Messages;

/// <summary>
/// Represents a message containing invoice details, including saved and total amounts,
/// along with an optional message providing additional information.
/// </summary>
public class InvoiceMessage
{
    public int Saved { get; set; }
    public int Total { get; set; }
    public string? Message { get; set; }

    // Private constructor to prevent direct instantiation.
    private InvoiceMessage()
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="InvoiceMessage"/> class with the specified values.
    /// </summary>
    /// <param name="saved">The saved value indicating the saved amount.</param>
    /// <param name="total">The total value indicating the total amount.</param>
    /// <param name="message">An optional message providing additional details.</param>
    /// <returns>Returns a new instance of the <see cref="InvoiceMessage"/> class populated with the provided values.</returns>
    public static InvoiceMessage Create(int saved, int total, string? message = null)
        => new()
        {
            Saved = saved,
            Total = total,
            Message = message
        };
}