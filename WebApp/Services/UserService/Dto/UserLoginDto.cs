﻿namespace WebApp.Services.UserService.Dto;

public class UserLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? OrgId { get; set; }
}