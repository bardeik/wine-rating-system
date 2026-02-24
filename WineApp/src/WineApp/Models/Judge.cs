using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineApp.Models;

public class Judge
{
    [DisplayName("Dommer Id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string JudgeId { get; set; } = ObjectId.GenerateNewId().ToString();

    [Required(ErrorMessage = "Navn er påkrevd")]
    [StringLength(100, ErrorMessage = "Navn kan ikke overstige 100 tegn")]
    [DisplayName("Navn")]
    public string Name { get; set; } = string.Empty;

    [DisplayName("Bruker Id")]
    public string? UserId { get; set; }
}
