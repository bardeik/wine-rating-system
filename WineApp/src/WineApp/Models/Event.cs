using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WineApp.Models;

public class Event
{
    [DisplayName("Arrangement Id")]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string EventId { get; set; } = ObjectId.GenerateNewId().ToString();

    [Required(ErrorMessage = "Navn er påkrevd")]
    [StringLength(200, ErrorMessage = "Navn kan ikke overstige 200 tegn")]
    [DisplayName("Arrangementsnavn")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "År er påkrevd")]
    [Range(2000, 2100, ErrorMessage = "År må være mellom 2000 og 2100")]
    [DisplayName("År")]
    public int Year { get; set; }

    [DisplayName("Påmeldingsstart")]
    public DateTime RegistrationStartDate { get; set; }

    [DisplayName("Påmeldingsslutt")]
    public DateTime RegistrationEndDate { get; set; }

    [DisplayName("Betalingsfrist")]
    public DateTime PaymentDeadline { get; set; }

    [DisplayName("Leveringsfrist")]
    public DateTime DeliveryDeadline { get; set; }

    [Required(ErrorMessage = "Avgift per vin er påkrevd")]
    [Range(0, 100000, ErrorMessage = "Avgift må være mellom 0 og 100000")]
    [DisplayName("Avgift per vin (NOK)")]
    public decimal FeePerWine { get; set; }

    [Required(ErrorMessage = "Banknavn er påkrevd")]
    [StringLength(200, ErrorMessage = "Banknavn kan ikke overstige 200 tegn")]
    [DisplayName("Banknavn")]
    public string BankName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kontonummer er påkrevd")]
    [StringLength(50, ErrorMessage = "Kontonummer kan ikke overstige 50 tegn")]
    [DisplayName("Kontonummer")]
    public string AccountNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "IBAN er påkrevd")]
    [StringLength(50, ErrorMessage = "IBAN kan ikke overstige 50 tegn")]
    [DisplayName("IBAN")]
    public string IBAN { get; set; } = string.Empty;

    [Required(ErrorMessage = "BIC/SWIFT er påkrevd")]
    [StringLength(20, ErrorMessage = "BIC/SWIFT kan ikke overstige 20 tegn")]
    [DisplayName("BIC/SWIFT")]
    public string BIC { get; set; } = string.Empty;

    [Required(ErrorMessage = "Organisasjonsnummer er påkrevd")]
    [StringLength(20, ErrorMessage = "Organisasjonsnummer kan ikke overstige 20 tegn")]
    [DisplayName("Organisasjonsnummer")]
    public string OrganizationNumber { get; set; } = string.Empty;

    [DisplayName("Leveringsadresse Norge")]
    [StringLength(500, ErrorMessage = "Leveringsadresse kan ikke overstige 500 tegn")]
    public string DeliveryAddressNorway { get; set; } = string.Empty;

    [DisplayName("Importørinfo for nordiske gjester")]
    [StringLength(500, ErrorMessage = "Importørinfo kan ikke overstige 500 tegn")]
    public string ImporterInfoNordic { get; set; } = string.Empty;

    [DisplayName("Gullgrense")]
    [Range(0, 20, ErrorMessage = "Gullgrense må være mellom 0 og 20")]
    public decimal GoldThreshold { get; set; } = 17.0m;

    [DisplayName("Sølvgrense")]
    [Range(0, 20, ErrorMessage = "Sølvgrense må være mellom 0 og 20")]
    public decimal SilverThreshold { get; set; } = 15.5m;

    [DisplayName("Bronsegrense")]
    [Range(0, 20, ErrorMessage = "Bronsegrense må være mellom 0 og 20")]
    public decimal BronzeThreshold { get; set; } = 14.0m;

    [DisplayName("Særlig utmerkelse grense")]
    [Range(0, 20, ErrorMessage = "Særlig utmerkelse grense må være mellom 0 og 20")]
    public decimal SpecialMeritThreshold { get; set; } = 12.0m;

    [DisplayName("Alternativ gullgrense")]
    [Range(0, 20, ErrorMessage = "Alternativ gullgrense må være mellom 0 og 20")]
    public decimal AdjustedGoldThreshold { get; set; } = 15.0m;

    [DisplayName("Alternativ sølvgrense")]
    [Range(0, 20, ErrorMessage = "Alternativ sølvgrense må være mellom 0 og 20")]
    public decimal AdjustedSilverThreshold { get; set; } = 14.0m;

    [DisplayName("Alternativ bronsegrense")]
    [Range(0, 20, ErrorMessage = "Alternativ bronsegrense må være mellom 0 og 20")]
    public decimal AdjustedBronzeThreshold { get; set; } = 13.0m;

    [DisplayName("Alternativ særlig utmerkelse grense")]
    [Range(0, 20, ErrorMessage = "Alternativ særlig utmerkelse grense må være mellom 0 og 20")]
    public decimal AdjustedSpecialMeritThreshold { get; set; } = 11.5m;

    [DisplayName("Gate-verdi utseende")]
    [Range(0.1, 3, ErrorMessage = "Gate-verdi utseende må være mellom 0,1 og 3,0")]
    public decimal AppearanceGateValue { get; set; } = 1.8m;

    [DisplayName("Gate-verdi nese")]
    [Range(0.1, 4, ErrorMessage = "Gate-verdi nese må være mellom 0,1 og 4,0")]
    public decimal NoseGateValue { get; set; } = 1.8m;

    [DisplayName("Gate-verdi smak")]
    [Range(0.1, 13, ErrorMessage = "Gate-verdi smak må være mellom 0,1 og 13,0")]
    public decimal TasteGateValue { get; set; } = 5.8m;

    [DisplayName("Avviksterskel")]
    [Range(0, 20, ErrorMessage = "Avviksterskel må være mellom 0 og 20")]
    public decimal OutlierThreshold { get; set; } = 4.0m;

    [DisplayName("Bruk alternative grenser")]
    public bool UseAdjustedThresholds { get; set; }

    [DisplayName("Aktiv")]
    public bool IsActive { get; set; } = true;

    [DisplayName("Opprettet")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
