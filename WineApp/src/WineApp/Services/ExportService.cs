using System.Text;
using WineApp.Models;

namespace WineApp.Services;

public interface IExportService
{
    /// <summary>
    /// Exports results to CSV format
    /// </summary>
    string ExportResultsToCSV(List<WineResult> results, List<Wine> wines, List<WineProducer> producers);
    
    /// <summary>
    /// Exports trophy winners to CSV format
    /// </summary>
    string ExportTrophiesToCSV(
        (Wine? wine, WineResult? result) aaretsVinbonde,
        (Wine? wine, WineResult? result) bestNorwegian,
        (Wine? wine, WineResult? result) bestNordic,
        List<WineProducer> producers);
    
    /// <summary>
    /// Exports complete event data (wines, ratings, results)
    /// </summary>
    string ExportEventData(
        Event eventData,
        List<Wine> wines,
        List<WineRating> ratings,
        List<WineResult> results,
        List<WineProducer> producers);
    
    /// <summary>
    /// Exports flight lists for printing
    /// </summary>
    string ExportFlightList(List<Flight> flights, List<Wine> wines);
    
    /// <summary>
    /// Downloads CSV content as file (returns bytes)
    /// </summary>
    byte[] GetCSVBytes(string csvContent);
}

public class ExportService : IExportService
{
    public string ExportResultsToCSV(List<WineResult> results, List<Wine> wines, List<WineProducer> producers)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Vinnummer,Vinnavn,Produsent,Gruppe,Klasse,Kategori,Total Score,Utseende,Nese,Smak,Klassifisering,Antall Vurderinger,Spread,Defekt,Outlier,Loddtrekning");
        
        // Data rows
        foreach (var result in results.OrderByDescending(r => r.TotalScore))
        {
            var wine = wines.FirstOrDefault(w => w.WineId == result.WineId);
            if (wine == null) continue;
            
            var producer = producers.FirstOrDefault(p => p.WineProducerId == wine.WineProducerId);
            
            sb.AppendLine(string.Join(",",
                EscapeCSV(wine.WineNumber?.ToString() ?? ""),
                EscapeCSV(wine.Name),
                EscapeCSV(producer?.WineyardName ?? ""),
                EscapeCSV(wine.Group.ToString()),
                EscapeCSV(wine.Class.ToString()),
                EscapeCSV(wine.Category.ToString()),
                result.TotalScore.ToString("F1"),
                result.AverageAppearance.ToString("F2"),
                result.AverageNose.ToString("F2"),
                result.AverageTaste.ToString("F2"),
                EscapeCSV(result.Classification),
                result.NumberOfRatings.ToString(),
                result.Spread.ToString("F1"),
                result.IsDefective ? "Ja" : "Nei",
                result.IsOutlier ? "Ja" : "Nei",
                result.RequiresLottery ? "Ja" : "Nei"
            ));
        }
        
        return sb.ToString();
    }

    public string ExportTrophiesToCSV(
        (Wine? wine, WineResult? result) aaretsVinbonde,
        (Wine? wine, WineResult? result) bestNorwegian,
        (Wine? wine, WineResult? result) bestNordic,
        List<WineProducer> producers)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine("Pokal,Vinnummer,Vinnavn,Produsent,Gruppe,Land,Årgang,Total Score,Klassifisering,Loddtrekning");
        
        // Årets Vinbonde
        if (aaretsVinbonde.wine != null && aaretsVinbonde.result != null)
        {
            var producer = producers.FirstOrDefault(p => p.WineProducerId == aaretsVinbonde.wine.WineProducerId);
            sb.AppendLine(string.Join(",",
                "Årets Vinbonde",
                aaretsVinbonde.wine.WineNumber?.ToString() ?? "",
                EscapeCSV(aaretsVinbonde.wine.Name),
                EscapeCSV(producer?.WineyardName ?? ""),
                aaretsVinbonde.wine.Group.ToString(),
                EscapeCSV(aaretsVinbonde.wine.Country),
                aaretsVinbonde.wine.Vintage.ToString(),
                aaretsVinbonde.result.TotalScore.ToString("F1"),
                EscapeCSV(aaretsVinbonde.result.Classification),
                aaretsVinbonde.result.RequiresLottery ? "Ja" : "Nei"
            ));
        }
        
        // Beste Norske
        if (bestNorwegian.wine != null && bestNorwegian.result != null)
        {
            var producer = producers.FirstOrDefault(p => p.WineProducerId == bestNorwegian.wine.WineProducerId);
            sb.AppendLine(string.Join(",",
                "Beste Norske Vin",
                bestNorwegian.wine.WineNumber?.ToString() ?? "",
                EscapeCSV(bestNorwegian.wine.Name),
                EscapeCSV(producer?.WineyardName ?? ""),
                bestNorwegian.wine.Group.ToString(),
                EscapeCSV(bestNorwegian.wine.Country),
                bestNorwegian.wine.Vintage.ToString(),
                bestNorwegian.result.TotalScore.ToString("F1"),
                EscapeCSV(bestNorwegian.result.Classification),
                bestNorwegian.result.RequiresLottery ? "Ja" : "Nei"
            ));
        }
        
        // Beste Nordiske
        if (bestNordic.wine != null && bestNordic.result != null)
        {
            var producer = producers.FirstOrDefault(p => p.WineProducerId == bestNordic.wine.WineProducerId);
            sb.AppendLine(string.Join(",",
                "Beste Nordiske Vin",
                bestNordic.wine.WineNumber?.ToString() ?? "",
                EscapeCSV(bestNordic.wine.Name),
                EscapeCSV(producer?.WineyardName ?? ""),
                bestNordic.wine.Group.ToString(),
                EscapeCSV(bestNordic.wine.Country),
                bestNordic.wine.Vintage.ToString(),
                bestNordic.result.TotalScore.ToString("F1"),
                EscapeCSV(bestNordic.result.Classification),
                bestNordic.result.RequiresLottery ? "Ja" : "Nei"
            ));
        }
        
        return sb.ToString();
    }

    public string ExportEventData(
        Event eventData,
        List<Wine> wines,
        List<WineRating> ratings,
        List<WineResult> results,
        List<WineProducer> producers)
    {
        var sb = new StringBuilder();
        
        // Event metadata
        sb.AppendLine($"# {eventData.Name} - Komplett data-eksport");
        sb.AppendLine($"# Eksportert: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        
        // Event info
        sb.AppendLine("## ARRANGEMENT");
        sb.AppendLine($"Navn,{EscapeCSV(eventData.Name)}");
        sb.AppendLine($"År,{eventData.Year}");
        sb.AppendLine($"Påmeldingsfrist,{eventData.RegistrationEndDate:yyyy-MM-dd}");
        sb.AppendLine($"Betalingsfrist,{eventData.PaymentDeadline:yyyy-MM-dd}");
        sb.AppendLine($"Leveringsfrist,{eventData.DeliveryDeadline:yyyy-MM-dd}");
        sb.AppendLine($"Avgift per vin,{eventData.FeePerWine}");
        sb.AppendLine();
        
        // Wines
        sb.AppendLine("## VINER");
        sb.AppendLine("Vinnummer,Navn,Vurderingsnavn,Produsent,Gruppe,Klasse,Kategori,Årgang,Alkohol%,Land,Vinbonde,Betalt");
        foreach (var wine in wines.OrderBy(w => w.WineNumber))
        {
            var producer = producers.FirstOrDefault(p => p.WineProducerId == wine.WineProducerId);
            sb.AppendLine(string.Join(",",
                wine.WineNumber?.ToString() ?? "",
                EscapeCSV(wine.Name),
                EscapeCSV(wine.RatingName),
                EscapeCSV(producer?.WineyardName ?? ""),
                wine.Group.ToString(),
                wine.Class.ToString(),
                wine.Category.ToString(),
                wine.Vintage.ToString(),
                wine.AlcoholPercentage.ToString("F1"),
                EscapeCSV(wine.Country),
                wine.IsVinbonde ? "Ja" : "Nei",
                wine.IsPaid ? "Ja" : "Nei"
            ));
        }
        sb.AppendLine();
        
        // Ratings
        sb.AppendLine("## VURDERINGER");
        sb.AppendLine("Vinnummer,Dommer,Utseende,Nese,Smak,Total,Kommentar,Dato");
        foreach (var rating in ratings.OrderBy(r => r.WineId).ThenBy(r => r.JudgeId))
        {
            var wine = wines.FirstOrDefault(w => w.WineId == rating.WineId);
            var total = rating.Appearance + rating.Nose + rating.Taste;
            sb.AppendLine(string.Join(",",
                wine?.WineNumber?.ToString() ?? "",
                EscapeCSV(rating.JudgeId),
                rating.Appearance.ToString("F1"),
                rating.Nose.ToString("F1"),
                rating.Taste.ToString("F1"),
                total.ToString("F1"),
                EscapeCSV(rating.Comment ?? ""),
                rating.RatingDate.ToString("yyyy-MM-dd HH:mm")
            ));
        }
        sb.AppendLine();
        
        // Results
        sb.AppendLine("## RESULTATER");
        sb.AppendLine("Vinnummer,Vinnavn,Total Score,Klassifisering,Antall Vurderinger,Spread,Defekt,Outlier");
        foreach (var result in results.OrderByDescending(r => r.TotalScore))
        {
            var wine = wines.FirstOrDefault(w => w.WineId == result.WineId);
            sb.AppendLine(string.Join(",",
                wine?.WineNumber?.ToString() ?? "",
                EscapeCSV(wine?.Name ?? ""),
                result.TotalScore.ToString("F1"),
                EscapeCSV(result.Classification),
                result.NumberOfRatings.ToString(),
                result.Spread.ToString("F1"),
                result.IsDefective ? "Ja" : "Nei",
                result.IsOutlier ? "Ja" : "Nei"
            ));
        }
        
        return sb.ToString();
    }

    public string ExportFlightList(List<Flight> flights, List<Wine> wines)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Flight,Vinnummer,Vinnavn,Kategori,Gruppe,Klasse");
        
        foreach (var flight in flights.OrderBy(f => f.FlightNumber))
        {
            var flightWines = flight.WineIds
                .Select(wineId => wines.FirstOrDefault(w => w.WineId == wineId))
                .Where(w => w != null)
                .OrderBy(w => w!.WineNumber);
            
            foreach (var wine in flightWines)
            {
                sb.AppendLine(string.Join(",",
                    EscapeCSV(flight.FlightName),
                    wine!.WineNumber?.ToString() ?? "",
                    EscapeCSV(wine.RatingName),
                    wine.Category.ToString(),
                    wine.Group.ToString(),
                    wine.Class.ToString()
                ));
            }
        }
        
        return sb.ToString();
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

    private string EscapeCSV(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return "";
        
        // Escape quotes and wrap in quotes if contains comma, quote, or newline
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        
        return value;
    }
}
