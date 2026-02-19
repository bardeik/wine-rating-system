using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class WineRating
{
    [DisplayName("Vurdering Id")]
    public int WineRatingId { get; set; }

    [Required]
    [Range(0, 10, ErrorMessage = "Utseende må være mellom 0 og 10")]
    [DisplayName("Utseende")]
    public int Visuality { get; set; }

    [Required]
    [Range(0, 10, ErrorMessage = "Nese må være mellom 0 og 10")]
    [DisplayName("Nese")]
    public int Nose { get; set; }

    [Required]
    [Range(0, 10, ErrorMessage = "Smak må være mellom 0 og 10")]
    [DisplayName("Smak")]
    public int Taste { get; set; }

    [Required(ErrorMessage = "Dommer-ID er påkrevd")]
    [StringLength(50, ErrorMessage = "Dommer-ID kan ikke overstige 50 tegn")]
    [DisplayName("Dommer")]
    public string JudgeId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vin-ID er påkrevd")]
    [Range(1, int.MaxValue, ErrorMessage = "Velg en gyldig vin")]
    [DisplayName("Vin")]
    public int WineId { get; set; }
}
