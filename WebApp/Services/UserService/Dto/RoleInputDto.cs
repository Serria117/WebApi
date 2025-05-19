using System.ComponentModel.DataAnnotations;

namespace WebApp.Services.UserService.Dto
{
    public class RoleInputDto
    {
        [Required][MaxLength(255), MinLength(3)]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ISet<int> Permissions { get; set; } = new HashSet<int>();
        public ISet<Guid> Users { get; set; } = new HashSet<Guid>();
    }
    
    public class RoleUpdatetDto : RoleInputDto
    {
        public new string? RoleName { get; set; }
    }
}
