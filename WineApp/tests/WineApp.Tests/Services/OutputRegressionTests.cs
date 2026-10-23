using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace WineApp.Tests.Services;

public class OutputRegressionTests
{
    [Theory]
    [InlineData("=1+1")]
    [InlineData("+1+1")]
    [InlineData("-1+1")]
    [InlineData("@SUM(1,1)")]
    [InlineData("\t=1+1")]
    [InlineData("\r=1+1")]
    [InlineData("\n=1+1")]
    public void ResultsCsv_EscapesFormulaPrefixesAndRoundTripsFields(string value)
    {
        var service = new ExportService(FrozenTimeProvider.At(2026, 10, 23));
        var csv = service.ExportResultsToCSV(
            [new WineResult { WineId = "wine", TotalScore = 17.5m, Classification = Classification.Gold }],
            [new Wine { WineId = "wine", WineProducerId = "producer", Name = value, WineNumber = 7 }],
            [new WineProducer { WineProducerId = "producer", WineyardName = "Vineyard; with separator" }]);
        using var input = new StringReader(csv);
        using var reader = new CsvReader(input, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";" });
        reader.Read().ShouldBeTrue();
        reader.ReadHeader();
        reader.Read().ShouldBeTrue();
        reader.GetField("Vinnavn").ShouldBe("'" + value);
        reader.GetField("Produsent").ShouldBe("Vineyard; with separator");
        reader.GetField("Vinnummer").ShouldBe("7");
    }

    [Fact]
    public void CsvDownload_ContainsUtf8BomAndOriginalContent()
    {
        var service = new ExportService(FrozenTimeProvider.At(2026, 10, 23));
        var bytes = service.GetCSVBytes("wine;score\r\nExample;17.5");
        bytes.Take(3).ShouldBe(Encoding.UTF8.GetPreamble());
        Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3).ShouldBe("wine;score\r\nExample;17.5");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TrophyPdf_GeneratesValidDocumentWithOrWithoutWinner(bool hasWinner)
    {
        var service = new PdfService(FrozenTimeProvider.At(2026, 10, 23));
        var evt = new Event { Name = "Test competition", Year = 2026 };
        var wine = new Wine { Name = "Example wine", WineProducerId = "producer", WineNumber = 1, Vintage = 2025 };
        var result = new WineResult { TotalScore = 18m, Classification = Classification.Gold, NumberOfRatings = 5 };
        var winner = hasWinner ? (wine, result) : ((Wine?)null, (WineResult?)null);
        var bytes = service.GenerateTrophyReport(evt, winner, winner, winner,
            [new WineProducer { WineProducerId = "producer", WineyardName = "Example vineyard" }]);
        bytes.Length.ShouldBeGreaterThan(1000);
        Encoding.ASCII.GetString(bytes, 0, 5).ShouldBe("%PDF-");
    }
}