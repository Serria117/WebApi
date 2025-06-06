﻿namespace WebApp.Payloads;

public class AuthenticationResponse
{
    public bool Success { get; set; } = false;
    public Guid Id { get; set; }
    public string? Message { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? Username { get; set; }
    public DateTime? IssueAt { get; set; }
    public DateTime? ExpireAt { get; set; }
    public string? WorkingTaxId { get; set; }
    public string? WorkingOrgId { get; set; }
    public string? WorkingOrgFullName { get; set; }
    public string? WorkingOrgShortName { get; set; }
    
}