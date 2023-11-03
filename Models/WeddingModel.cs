#pragma warning disable CS8618
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace wedding_planner.Models;
public class Wedding
{
    [Key]
    public int WeddingId { get; set; }
    [Required(ErrorMessage = "El Nombre del Conyuge es requerido.")]
    public string WedderOne { get; set; }
    [Required(ErrorMessage = "El Nombre del Conyuge es requerido.")]
    public string WedderTwo { get; set; }
    [Required(ErrorMessage = "La fecha de la boda es requerida.")]
    [FutureDate]
    public DateTime WeddingDate { get; set; }
    [Required(ErrorMessage = "La dirección de la boda es requerida.")]
    public string WeddingAddress { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public int UserId { get; set; }
    public User? Creador { get; set; }
    public List<Invitation> Invitados { get; set; } = new List<Invitation>();
}

public class FutureDateAttribute : ValidationAttribute
{
    public override string FormatErrorMessage(string name)
    {
        return "La fecha de la boda debe ser futura.";
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime date)
        {
            if (date.Date >= DateTime.Now.Date)
            {
                return ValidationResult.Success;
            }
        }

        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }
}

public class WeddingViewModel
{
    public Wedding Wedding { get; set; }
    public bool IsInvited { get; set; }
    public int GuestCount { get; set; }
}
