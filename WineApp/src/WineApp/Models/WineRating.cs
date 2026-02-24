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
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string WineRatingId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

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
    [DisplayName("Vin")]
    public string WineId { get; set; } = string.Empty;
}
