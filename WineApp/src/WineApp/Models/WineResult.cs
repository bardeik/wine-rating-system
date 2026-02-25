using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WineApp.Models;

public class WineResult
{
    [DisplayName("Resultat Id")]
    [MongoDB.Bson.Serialization.Attributes.BsonId]
    [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
    public string WineResultId { get; set; } = MongoDB.Bson.ObjectId.GenerateNewId().ToString();

    [Required(ErrorMessage = "Vin-ID er påkrevd")]
    [DisplayName("Vin")]
    public string WineId { get; set; } = string.Empty;

    [DisplayName("Gjennomsnitt utseende (A)")]
    public decimal AverageAppearance { get; set; }

    [DisplayName("Gjennomsnitt nese (B)")]
    public decimal AverageNose { get; set; }

    [DisplayName("Gjennomsnitt smak (C)")]
    public decimal AverageTaste { get; set; }

    [DisplayName("Sum av gjennomsnitt")]
    public decimal TotalScore { get; set; }

    [DisplayName("Klassifisering")]
    [StringLength(50, ErrorMessage = "Klassifisering kan ikke overstige 50 tegn")]
    public string Classification { get; set; } = string.Empty;

    [DisplayName("Feilbeheftet/defekt")]
    public bool IsDefective { get; set; }

    [DisplayName("Avviksflagg (>4.0 spredning)")]
    public bool IsOutlier { get; set; }

    [DisplayName("Spredning (maks-min)")]
    public decimal Spread { get; set; }

    [DisplayName("Høyeste enkeltpoeng")]
    public decimal HighestSingleScore { get; set; }

    [DisplayName("Dommer med høyeste enkeltpoeng")]
    public string? HighestScoreJudgeId { get; set; }

    [DisplayName("Oppfyller gate-verdier")]
    public bool MeetsGateValues { get; set; }

    [DisplayName("Beregningsdato")]
    public DateTime CalculationDate { get; set; } = DateTime.UtcNow;

    [DisplayName("Krever loddtrekning")]
    public bool RequiresLottery { get; set; }

    [DisplayName("Antall dommervurderinger")]
    public int NumberOfRatings { get; set; }
}
