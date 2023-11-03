#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wedding_planner.Models;
public class User
{
    [Key]
    public int UserId { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "El apellido es requerido.")]
    [MinLength(2, ErrorMessage = "El apellido debe tener al menos 2 caracteres.")]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [UniqueEmail]
    public string Email { get; set; }

    [Required(ErrorMessage = "Debes ingresar una contraseña.")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    [NotMapped]
    [Compare("Password")]
    public string PasswordConfirm { get; set; }

    public List<Invitation> Invitaciones { get; set; } = new List<Invitation>();
}

public class UniqueEmailAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
        {
            return new ValidationResult("El correo es requerido");
        }

        MyContext _context = (MyContext)validationContext.GetService(typeof(MyContext));
        if (_context.Users.Any(e => e.Email == value.ToString()))
        {
            return new ValidationResult("Este correo ya esta registrado");
        }
        else
        {
            return ValidationResult.Success;
        }
    }
}