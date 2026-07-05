using FF_ModelsDB.Models;

namespace FF_Mvc.ViewModels;

public class UserViewModel
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int RoleId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public Role Role { get; set; } = null!;
}

