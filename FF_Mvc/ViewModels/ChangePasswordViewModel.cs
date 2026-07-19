using System.ComponentModel.DataAnnotations;

namespace FF_Mvc.ViewModels;

public class ChangePasswordViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    [RegularExpression(@"^(?=.*[a-zA-Z])\S+$", ErrorMessage = "La contraseña debe contener al menos una letra y no puede tener espacios.")]
    public string NewPassword { get; set; } = null!;

    [Required(ErrorMessage = "Debe confirmar la contraseña.")]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = null!;
}
