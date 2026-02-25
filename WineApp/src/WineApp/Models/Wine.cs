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
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string WineId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

    [DisplayName("Vinnummer")]
    public int? WineNumber { get; set; }

    [Required(ErrorMessage = "Vurderingsnavn er påkrevd")]
    [StringLength(100, ErrorMessage = "Vurderingsnavn kan ikke overstige 100 tegn")]
    [DisplayName("Vurderingsnavn")]
    public string RatingName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Navn er påkrevd")]
    [StringLength(100, ErrorMessage = "Navn kan ikke overstige 100 tegn")]
    [DisplayName("Navn")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Årgang er påkrevd")]
    [Range(1900, 2100, ErrorMessage = "Årgang må være mellom 1900 og 2100")]
    [DisplayName("Årgang")]
    public int Vintage { get; set; }

    [Required(ErrorMessage = "Alkoholprosent er påkrevd")]
    [Range(0.0, 100.0, ErrorMessage = "Alkoholprosent må være mellom 0 og 100")]
    [DisplayName("Alkohol %")]
    public decimal AlcoholPercentage { get; set; }

    [DisplayName("Drueblanding (%)")]
    public Dictionary<string, decimal> GrapeBlend { get; set; } = new Dictionary<string, decimal>();

    [DisplayName("Vinbonde (≥100 vinstokker)")]
    public bool IsVinbonde { get; set; }

    [Required(ErrorMessage = "Land er påkrevd")]
    [StringLength(100, ErrorMessage = "Land kan ikke overstige 100 tegn")]
    [DisplayName("Land")]
    public string Country { get; set; } = "Norge";

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
    [DisplayName("Vinprodusent")]
    public string WineProducerId { get; set; } = string.Empty;

    [DisplayName("Arrangement")]
    public string? EventId { get; set; }

    [DisplayName("Betalingsstatus")]
    public bool IsPaid { get; set; }

    [DisplayName("Påmeldingsdato")]
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
}
