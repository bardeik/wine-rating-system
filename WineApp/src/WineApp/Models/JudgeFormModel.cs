using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class JudgeFormModel
{
    [Required(ErrorMessage = "Navn er påkrevd")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-post er påkrevd")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Passord er påkrevd")]
    [MinLength(8, ErrorMessage = "Passord må være minst 8 tegn")]
    public string Password { get; set; } = string.Empty;
}
