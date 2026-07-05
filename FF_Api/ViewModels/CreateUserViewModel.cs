using System.ComponentModel.DataAnnotations;

namespace FF_Api.ViewModels
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [RegularExpression(@"^\S+$", ErrorMessage = "El nombre de usuario no puede contener espacios.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Debe seleccionar un rol.")]
        public int RoleId { get; set; }
    }

}
