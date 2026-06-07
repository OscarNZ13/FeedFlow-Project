using FF_ModelsDB.Models;

namespace FF_Api.ViewModels;

public class RoleViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}