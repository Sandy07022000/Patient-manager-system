using GestorPacientes.Core.Application.Enums;
using System.ComponentModel.DataAnnotations;

namespace GestorPacientes.Core.Application.ViewModels.Users
{
    public class SaveUserViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El {0} es requerido")]
        [Display(Name = "Nombre de usuario")]
        [DataType(DataType.Text)]
        public string Username { get; set; } 
        [Required(ErrorMessage = "La {0} es requerida")]
        [Display(Name = "Contraseña")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
        [Required(ErrorMessage = "{0} es requerido")]
        [Display(Name = "Confirmar contraseña")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessage = "El {0} es requerido")]
        [Display(Name = "Nombre")]
        [DataType(DataType.Text)]
        public string Name { get; set; }
        [Required(ErrorMessage = "El {0} es requerido")]
        [Display(Name = "Apellido")]
        [DataType(DataType.Text)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "El {0} es requerido")]
        [Display(Name = "Correo electronico")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required(ErrorMessage = "El {0} es requerido")]
        [Display(Name = "Telefono")]
        public string? Phone { get; set; }
        [Required(ErrorMessage = "Debe seleccionar un tipo de usuario")]
        [Display(Name = "Tipo de usuario")]
        public Roles? TypeUserId { get; set; }
    }
}
