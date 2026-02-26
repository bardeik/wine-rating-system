using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineApp.Models;

public class Payment
{
    [DisplayName("Betaling Id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PaymentId { get; set; } = ObjectId.GenerateNewId().ToString();

    [Required(ErrorMessage = "Vinprodusent er påkrevd")]
    [DisplayName("Vinprodusent")]
    public string WineProducerId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Arrangement er påkrevd")]
    [DisplayName("Arrangement")]
    public string EventId { get; set; } = string.Empty;

    [DisplayName("Vin-IDer")]
    public List<string> WineIds { get; set; } = new List<string>();

    [Required(ErrorMessage = "Beløp er påkrevd")]
    [Range(0, 1000000, ErrorMessage = "Beløp må være mellom 0 og 1000000")]
    [DisplayName("Beløp (NOK)")]
    public decimal Amount { get; set; }

    [DisplayName("Antall viner")]
    public int NumberOfWines { get; set; }

    [DisplayName("Betalt")]
    public bool IsPaid { get; set; }

    [DisplayName("Betalingsdato")]
    public DateTime? PaymentDate { get; set; }

    [DisplayName("Betalingsreferanse")]
    [StringLength(100, ErrorMessage = "Betalingsreferanse kan ikke overstige 100 tegn")]
    public string PaymentReference { get; set; } = string.Empty;

    [DisplayName("Kvittering sendt")]
    public bool ReceiptSent { get; set; }

    [DisplayName("Kvitteringsdato")]
    public DateTime? ReceiptSentDate { get; set; }

    [DisplayName("Registreringsdato")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [DisplayName("Registrert av")]
    [StringLength(100, ErrorMessage = "Registrert av kan ikke overstige 100 tegn")]
    public string RegisteredBy { get; set; } = string.Empty;

    [DisplayName("Notater")]
    [StringLength(500, ErrorMessage = "Notater kan ikke overstige 500 tegn")]
    public string Notes { get; set; } = string.Empty;
}
