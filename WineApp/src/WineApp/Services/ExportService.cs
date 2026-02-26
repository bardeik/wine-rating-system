using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using WineApp.Models;

namespace WineApp.Services;

public class ExportService : IExportService
{
    private static readonly CsvConfiguration CsvConfig = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
    };

    private static readonly CsvConfiguration CsvConfigNoHeader = new(CultureInfo.InvariantCulture)
    {
        Delimiter = ";",
        HasHeaderRecord = false,
    };

    public string ExportResultsToCSV(List<WineResult> results, List<Wine> wines, List<WineProducer> producers)
    {
        var rows = results
            .OrderByDescending(r => r.TotalScore)
            .Select(result =>
            {
                var wine = wines.FirstOrDefault(w => w.WineId == result.WineId);
                var producer = wine != null ? producers.FirstOrDefault(p => p.WineProducerId == wine.WineProducerId) : null;
                return new WineResultRow(
                    wine?.WineNumber?.ToString() ?? "",
                    wine?.Name ?? "",
                    producer?.WineyardName ?? "",
                    wine?.Group.ToString() ?? "",
                    wine?.Class.ToString() ?? "",
                    wine?.Category.ToString() ?? "",
                    result.TotalScore.ToString("F1"),
                    result.AverageAppearance.ToString("F2"),
                    result.AverageNose.ToString("F2"),
                    result.AverageTaste.ToString("F2"),
                    result.Classification,
                    result.NumberOfRatings.ToString(),
                    result.Spread.ToString("F1"),
                    result.IsDefective ? "Ja" : "Nei",
                    result.IsOutlier ? "Ja" : "Nei",
                    result.RequiresLottery ? "Ja" : "Nei"
                );
            });

        return WriteRecords(rows);
    }

    public string ExportTrophiesToCSV(
        (Wine? wine, WineResult? result) aaretsVinbonde,
        (Wine? wine, WineResult? result) bestNorwegian,
        (Wine? wine, WineResult? result) bestNordic,
        List<WineProducer> producers)
    {
        var rows = new List<TrophyRow>();

        void AddRow(string trophy, Wine? wine, WineResult? result)
        {
            if (wine == null || result == null) return;
            var producer = producers.FirstOrDefault(p => p.WineProducerId == wine.WineProducerId);
            rows.Add(new TrophyRow(
                trophy,
                wine.WineNumber?.ToString() ?? "",
                wine.Name,
                producer?.WineyardName ?? "",
                wine.Group.ToString(),
                wine.Country,
                wine.Vintage.ToString(),
                result.TotalScore.ToString("F1"),
                result.Classification,
                result.RequiresLottery ? "Ja" : "Nei"
            ));
        }

        AddRow("Årets Vinbonde", aaretsVinbonde.wine, aaretsVinbonde.result);
        AddRow("Beste Norske Vin", bestNorwegian.wine, bestNorwegian.result);
        AddRow("Beste Nordiske Vin", bestNordic.wine, bestNordic.result);

        return WriteRecords(rows);
    }

    public string ExportEventData(
        Event eventData,
        List<Wine> wines,
        List<WineRating> ratings,
        List<WineResult> results,
        List<WineProducer> producers)
    {
        var sb = new StringBuilder();

        // Event metadata (narrative comment lines)
        sb.AppendLine($"# {eventData.Name} - Komplett data-eksport");
        sb.AppendLine($"# Eksportert: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Event info as key-value rows (no header row)
        sb.AppendLine("## ARRANGEMENT");
        sb.Append(WriteRecords(new[]
        {
            new KeyValueRow("Navn", eventData.Name),
            new KeyValueRow("År", eventData.Year.ToString()),
            new KeyValueRow("Påmeldingsfrist", eventData.RegistrationEndDate.ToString("yyyy-MM-dd")),
            new KeyValueRow("Betalingsfrist", eventData.PaymentDeadline.ToString("yyyy-MM-dd")),
            new KeyValueRow("Leveringsfrist", eventData.DeliveryDeadline.ToString("yyyy-MM-dd")),
            new KeyValueRow("Avgift per vin", eventData.FeePerWine.ToString()),
        }, CsvConfigNoHeader));
        sb.AppendLine();

        // Wines section
        sb.AppendLine("## VINER");
        var wineRows = wines.OrderBy(w => w.WineNumber).Select(wine =>
        {
            var producer = producers.FirstOrDefault(p => p.WineProducerId == wine.WineProducerId);
            return new EventWineRow(
                wine.WineNumber?.ToString() ?? "",
                wine.Name,
                wine.RatingName,
                producer?.WineyardName ?? "",
                wine.Group.ToString(),
                wine.Class.ToString(),
                wine.Category.ToString(),
                wine.Vintage.ToString(),
                wine.AlcoholPercentage.ToString("F1"),
                wine.Country,
                wine.IsVinbonde ? "Ja" : "Nei",
                wine.IsPaid ? "Ja" : "Nei"
            );
        });
        sb.Append(WriteRecords(wineRows));
        sb.AppendLine();

        // Ratings section
        sb.AppendLine("## VURDERINGER");
        var ratingRows = ratings.OrderBy(r => r.WineId).ThenBy(r => r.JudgeId).Select(rating =>
        {
            var wine = wines.FirstOrDefault(w => w.WineId == rating.WineId);
            var total = rating.Appearance + rating.Nose + rating.Taste;
            return new EventRatingRow(
                wine?.WineNumber?.ToString() ?? "",
                rating.JudgeId,
                rating.Appearance.ToString("F1"),
                rating.Nose.ToString("F1"),
                rating.Taste.ToString("F1"),
                total.ToString("F1"),
                rating.Comment ?? "",
                rating.RatingDate.ToString("yyyy-MM-dd HH:mm")
            );
        });
        sb.Append(WriteRecords(ratingRows));
        sb.AppendLine();

        // Results section
        sb.AppendLine("## RESULTATER");
        var resultRows = results.OrderByDescending(r => r.TotalScore).Select(result =>
        {
            var wine = wines.FirstOrDefault(w => w.WineId == result.WineId);
            return new EventResultRow(
                wine?.WineNumber?.ToString() ?? "",
                wine?.Name ?? "",
                result.TotalScore.ToString("F1"),
                result.Classification,
                result.NumberOfRatings.ToString(),
                result.Spread.ToString("F1"),
                result.IsDefective ? "Ja" : "Nei",
                result.IsOutlier ? "Ja" : "Nei"
            );
        });
        sb.Append(WriteRecords(resultRows));

        return sb.ToString();
    }

    public string ExportFlightList(List<Flight> flights, List<Wine> wines)
    {
        var rows = flights
            .OrderBy(f => f.FlightNumber)
            .SelectMany(flight =>
                flight.WineIds
                    .Select(wineId => wines.FirstOrDefault(w => w.WineId == wineId))
                    .Where(w => w != null)
                    .OrderBy(w => w!.WineNumber)
                    .Select(wine => new FlightListRow(
                        flight.FlightName,
                        wine!.WineNumber?.ToString() ?? "",
                        wine.RatingName,
                        wine.Category.ToString(),
                        wine.Group.ToString(),
                        wine.Class.ToString()
                    ))
            );

        return WriteRecords(rows);
    }

    public byte[] GetCSVBytes(string csvContent)
    {
        // Use UTF-8 with BOM for Excel compatibility
        var preamble = Encoding.UTF8.GetPreamble();
        var content = Encoding.UTF8.GetBytes(csvContent);
        var result = new byte[preamble.Length + content.Length];

        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(content, 0, result, preamble.Length, content.Length);

        return result;
    }

    private static string WriteRecords<T>(IEnumerable<T> records, CsvConfiguration? config = null)
    {
        using var sw = new StringWriter();
        using var csv = new CsvWriter(sw, config ?? CsvConfig);
        csv.WriteRecords(records);
        return sw.ToString();
    }
}

// ── Row record types ──────────────────────────────────────────────────────────

file sealed record WineResultRow(
    [property: Name("Vinnummer")] string WineNumber,
    [property: Name("Vinnavn")] string WineName,
    [property: Name("Produsent")] string Producer,
    [property: Name("Gruppe")] string Group,
    [property: Name("Klasse")] string Class,
    [property: Name("Kategori")] string Category,
    [property: Name("Total Score")] string TotalScore,
    [property: Name("Utseende")] string Appearance,
    [property: Name("Nese")] string Nose,
    [property: Name("Smak")] string Taste,
    [property: Name("Klassifisering")] string Classification,
    [property: Name("Antall Vurderinger")] string NumberOfRatings,
    [property: Name("Spread")] string Spread,
    [property: Name("Defekt")] string IsDefective,
    [property: Name("Outlier")] string IsOutlier,
    [property: Name("Loddtrekning")] string RequiresLottery
);

file sealed record TrophyRow(
    [property: Name("Pokal")] string Trophy,
    [property: Name("Vinnummer")] string WineNumber,
    [property: Name("Vinnavn")] string WineName,
    [property: Name("Produsent")] string Producer,
    [property: Name("Gruppe")] string Group,
    [property: Name("Land")] string Country,
    [property: Name("Årgang")] string Vintage,
    [property: Name("Total Score")] string TotalScore,
    [property: Name("Klassifisering")] string Classification,
    [property: Name("Loddtrekning")] string RequiresLottery
);

file sealed record KeyValueRow(string Key, string Value);

file sealed record EventWineRow(
    [property: Name("Vinnummer")] string WineNumber,
    [property: Name("Navn")] string Name,
    [property: Name("Vurderingsnavn")] string RatingName,
    [property: Name("Produsent")] string Producer,
    [property: Name("Gruppe")] string Group,
    [property: Name("Klasse")] string Class,
    [property: Name("Kategori")] string Category,
    [property: Name("Årgang")] string Vintage,
    [property: Name("Alkohol%")] string AlcoholPercentage,
    [property: Name("Land")] string Country,
    [property: Name("Vinbonde")] string IsVinbonde,
    [property: Name("Betalt")] string IsPaid
);

file sealed record EventRatingRow(
    [property: Name("Vinnummer")] string WineNumber,
    [property: Name("Dommer")] string JudgeId,
    [property: Name("Utseende")] string Appearance,
    [property: Name("Nese")] string Nose,
    [property: Name("Smak")] string Taste,
    [property: Name("Total")] string Total,
    [property: Name("Kommentar")] string Comment,
    [property: Name("Dato")] string RatingDate
);

file sealed record EventResultRow(
    [property: Name("Vinnummer")] string WineNumber,
    [property: Name("Vinnavn")] string WineName,
    [property: Name("Total Score")] string TotalScore,
    [property: Name("Klassifisering")] string Classification,
    [property: Name("Antall Vurderinger")] string NumberOfRatings,
    [property: Name("Spread")] string Spread,
    [property: Name("Defekt")] string IsDefective,
    [property: Name("Outlier")] string IsOutlier
);

file sealed record FlightListRow(
    [property: Name("Flight")] string FlightName,
    [property: Name("Vinnummer")] string WineNumber,
    [property: Name("Vinnavn")] string WineName,
    [property: Name("Kategori")] string Category,
    [property: Name("Gruppe")] string Group,
    [property: Name("Klasse")] string Class
);
