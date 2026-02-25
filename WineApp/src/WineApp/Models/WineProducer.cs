using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WineApp.Models;

public class WineProducer
{
    [DisplayName("Vinprodusent Id")]
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string WineProducerId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

    [StringLength(50, ErrorMessage = "Medlemsnummer kan ikke overstige 50 tegn")]
    [DisplayName("Medlemsnummer")]
    public string MemberNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vingårdsnavn er påkrevd")]
    [StringLength(100, ErrorMessage = "Vingårdsnavn kan ikke overstige 100 tegn")]
    [DisplayName("Vingårdsnavn")]
    public string WineyardName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Organisasjonsnummer er påkrevd")]
    [StringLength(20, ErrorMessage = "Organisasjonsnummer kan ikke overstige 20 tegn")]
    [DisplayName("Organisasjonsnummer")]
    public string OrganisationNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ansvarlig produsents navn er påkrevd")]
    [StringLength(100, ErrorMessage = "Navn kan ikke overstige 100 tegn")]
    [DisplayName("Ansvarlig produsent")]
    public string ResponsibleProducerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adresse er påkrevd")]
    [StringLength(200, ErrorMessage = "Adresse kan ikke overstige 200 tegn")]
    [DisplayName("Adresse")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "By er påkrevd")]
    [StringLength(100, ErrorMessage = "By kan ikke overstige 100 tegn")]
    [DisplayName("By")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Land er påkrevd")]
    [StringLength(100, ErrorMessage = "Land kan ikke overstige 100 tegn")]
    [DisplayName("Land")]
    public string Country { get; set; } = string.Empty;

    [Required(ErrorMessage = "Postnummer er påkrevd")]
    [StringLength(20, ErrorMessage = "Postnummer kan ikke overstige 20 tegn")]
    [DisplayName("Postnummer")]
    public string Zip { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon er påkrevd")]
    [Phone(ErrorMessage = "Ugyldig telefonnummer")]
    [StringLength(20, ErrorMessage = "Telefon kan ikke overstige 20 tegn")]
    [DisplayName("Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-post er påkrevd")]
    [EmailAddress(ErrorMessage = "Ugyldig e-postadresse")]
    [StringLength(100, ErrorMessage = "E-post kan ikke overstige 100 tegn")]
    [DisplayName("E-post")]
    public string Email { get; set; } = string.Empty;

    [DisplayName("Bruker Id")]
    public string? UserId { get; set; }
}
