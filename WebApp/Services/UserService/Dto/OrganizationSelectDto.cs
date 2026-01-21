namespace WebApp.Services.UserService.Dto;

public class OrganizationSelectDto
{
    public Guid Id { get; set; }
    public string ShortName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string UnsignName { get; set; } = string.Empty;
}
