using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class Wine
{
    [DisplayName("Vin Id")]
    public int WineId { get; set; }

    [Required(ErrorMessage = "Vurderingsnavn er påkrevd")]
    [StringLength(100, ErrorMessage = "Vurderingsnavn kan ikke overstige 100 tegn")]
    [DisplayName("Vurderingsnavn")]
    public string RatingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Navn er påkrevd")]
    [StringLength(100, ErrorMessage = "Navn kan ikke overstige 100 tegn")]
    [DisplayName("Navn")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gruppe er påkrevd")]
    [DisplayName("Gruppe")]
    public WineGroup Group { get; set; }

    [Required(ErrorMessage = "Klasse er påkrevd")]
    [DisplayName("Klasse")]
    public WineClass Class { get; set; }

    [Required(ErrorMessage = "Kategori er påkrevd")]
    [DisplayName("Kategori")]
    public WineCategory Category { get; set; }

    [Required(ErrorMessage = "Vinprodusent er påkrevd")]
    [Range(1, int.MaxValue, ErrorMessage = "Velg en gyldig vinprodusent")]
    [DisplayName("Vinprodusent")]
    public int WineProducerId { get; set; }
}
