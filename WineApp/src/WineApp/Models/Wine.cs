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
    [DisplayName("Vinprodusent")]
    public string WineProducerId { get; set; } = string.Empty;
}
