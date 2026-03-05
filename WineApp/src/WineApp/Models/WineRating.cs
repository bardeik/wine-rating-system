using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineApp.Models;

public class WineRating
{
    [DisplayName("Vurdering Id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string WineRatingId { get; set; } = ObjectId.GenerateNewId().ToString();

    [Required]
    [Range(0.0, 3.0, ErrorMessage = "Utseende (A) må være mellom 0.0 og 3.0")]
    [DisplayName("Utseende (A)")]
    public decimal Appearance { get; set; }

    [Required]
    [Range(0.0, 4.0, ErrorMessage = "Nese (B) må være mellom 0.0 og 4.0")]
    [DisplayName("Nese (B)")]
    public decimal Nose { get; set; }

    [Required]
    [Range(0.0, 13.0, ErrorMessage = "Smak (C) må være mellom 0.0 og 13.0")]
    [DisplayName("Smak (C)")]
    public decimal Taste { get; set; }

    [DisplayName("Kommentar")]
    [StringLength(1000, ErrorMessage = "Kommentar kan ikke overstige 1000 tegn")]
    public string Comment { get; set; } = string.Empty;

    [DisplayName("Total")]
    public decimal Total => Math.Round(Appearance + Nose + Taste, 1);

    [Required(ErrorMessage = "Dommer-ID er påkrevd")]
    [StringLength(50, ErrorMessage = "Dommer-ID kan ikke overstige 50 tegn")]
    [DisplayName("Dommer")]
    public string JudgeId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vin-ID er påkrevd")]
    [DisplayName("Vin")]
    public string WineId { get; set; } = string.Empty;

    [DisplayName("Vurderingsdato")]
    public DateTime RatingDate { get; set; }
}
