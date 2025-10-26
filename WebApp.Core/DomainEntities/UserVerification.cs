using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Core.DomainEntities;

[Index(nameof(VerificationKey))]
public class UserVerification : BaseEntityAuditable<long>
{
    [MaxLength(255)]
    public string VerificationKey { get; set; } = string.Empty;

    [MaxLength(10)]
    public string VerificationCode { get; set; } = string.Empty;

    public DateTime? VerificationCodeExpiration { get; set; }

    public Guid UserId { get; set; }
    
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;

    [EmailAddress][MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Ip { get; set; }

}