using System.ComponentModel.DataAnnotations;

namespace FF_Mvc.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "El usuario es requerido.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[a-zA-Z]).+$", ErrorMessage = "La contraseña debe contener al menos una letra.")]
    public string Password { get; set; } = null!;
}