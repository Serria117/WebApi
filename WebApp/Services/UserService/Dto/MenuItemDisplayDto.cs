namespace WebApp.Services.UserService.Dto;

public class MenuItemDisplayDto {
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; } = string.Empty;
    public string? To { get; set; }
    public int Order { get; set; }
    public int? ParentId { get; set; }
}