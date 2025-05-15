namespace WebApp.Services.UserService.Dto;

public class MenuInputDto
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? To { get; set; }
    public int Order { get; set; }
    public int? ParentId { get; set; }

    public List<int> Permissions { get; set; } = [];
}