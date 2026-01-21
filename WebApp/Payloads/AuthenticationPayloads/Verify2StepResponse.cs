namespace WebApp.Payloads.AuthenticationPayloads;

public class Verify2StepResponse
{
    public bool Success { get; set; } = false;
    public string? Message { get; set; }
    public AuthResponseCode ResponseCode { get; set; }
}

public enum AuthResponseCode
{
    Success = 0,
    AuthenticationTokenInvalid = 1,
    VerificationCodeInvalid = 2,
    EmptyInput = 3
}