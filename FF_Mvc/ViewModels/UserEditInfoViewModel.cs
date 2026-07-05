using System.ComponentModel.DataAnnotations;

namespace FF_Mvc.ViewModels;

public class EditInfoViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [RegularExpression(@"^\S+$", ErrorMessage = "El nombre de usuario no puede contener espacios.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Debe seleccionar un rol.")]
    [Range(1, 2, ErrorMessage = "Debe seleccionar un rol válido (1 = Administrador, 2 = Usuario).")]
    public int RoleId { get; set; }
}
