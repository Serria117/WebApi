using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.PayrollService.Dto;

public class DependentCreateDto
{
    [MaxLength(255, ErrorMessage = "Full name cannot exceed 255 characters.")]
    public string FullName { get; set; } = string.Empty;
    [MaxLength(20, ErrorMessage = "Tax ID cannot exceed 20 characters.")]
    public string? TaxId { get; set; }
    [MaxLength(20, ErrorMessage = "Employer ID cannot exceed 20 characters.")]
    public string? Pid { get; set; }
    [MaxLength(30)]
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid EmployeeId { get; set; } // Foreign key
}

public class DependentDisplayDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Pid { get; set; }
    public string Relationship { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid EmployeeId { get; set; }
    
}