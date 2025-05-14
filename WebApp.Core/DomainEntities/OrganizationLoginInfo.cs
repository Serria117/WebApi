using System.ComponentModel.DataAnnotations;

namespace WebApp.Core.DomainEntities;

public class OrganizationLoginInfo: BaseEntity<int>
{
    [MaxLength(50)]
    public string? AccountName { get; set; }
    [MaxLength(255)]
    public string? Url { get; set; }
    [MaxLength(50)]
    public string? Provider { get; set; }
    [MaxLength(50)]
    public string? Username { get; set; }
    [MaxLength(50)]
    public string? Password { get; set; }

    public Organization? Organization { get; set; }
    public Guid OrganizationId { get; set; }
}