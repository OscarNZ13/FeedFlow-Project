using System.ComponentModel.DataAnnotations;

namespace FF_Mvc.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = null!;
}