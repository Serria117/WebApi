using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.UserService.Dto
{
    public class RoleInputDto
    {
        [Required][MaxLength(255), MinLength(3)]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ISet<int> Permissions { get; set; } = new HashSet<int>();
        public ISet<string> User { get; set; } = new HashSet<string>();
    }
    
    public class RoleUpdatetDto : RoleInputDto
    {
        public new string? RoleName { get; set; }
    }
}
