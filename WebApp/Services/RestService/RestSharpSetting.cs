namespace WebApp.Services.RestService;

/// <summary>
/// Represents settings for configuring RestSharp, a library for making HTTP requests,
/// including base URL and cookies.
/// </summary>
public class RestSharpSetting
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Cookie { get; set; } = string.Empty;
    public string AuthCookie { get; set; } = string.Empty;
}
