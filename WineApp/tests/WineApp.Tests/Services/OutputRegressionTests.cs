using System.Text;
using CsvHelper;

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
    [InlineData("=SUM(1;2)\"quoted\"\r\nnext line")]
    public void ResultsCsv_EscapesFormulaPrefixesAndRoundTripsFields(string value)
    {
        var service = new ExportService(FrozenTimeProvider.At(2026, 10, 23));
        var csv = service.ExportResultsToCSV(
            [new WineResult { WineId = "wine", TotalScore = 17.5m, Classification = Classification.Gold }],
            [new Wine { WineId = "wine", WineProducerId = "producer", Name = value, WineNumber = 7 }],
            [new WineProducer { WineProducerId = "producer", WineyardName = "Vineyard; with separator" }]);
        using var input = new StringReader(csv);
        using var reader = CsvReader.Create(input, new CsvReaderOptions { Separator = ';' });
        reader.ReadHeader();
        reader.Read().ShouldBeTrue();
        reader.GetString("Vinnavn").ShouldBe("'" + value);
        reader.GetString("Produsent").ShouldBe("Vineyard; with separator");
        reader.GetString("Vinnummer").ShouldBe("7");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TrophyCsv_PreservesHeadersAndEscapesFields(bool hasWinner)
    {
        var service = new ExportService(FrozenTimeProvider.At(2026, 10, 23));
        var wine = new Wine { WineId = "wine", Name = "=1+1", WineNumber = 7, WineProducerId = "producer" };
        var result = new WineResult { Classification = Classification.Gold };
        var winner = hasWinner ? (wine, result) : ((Wine?)null, (WineResult?)null);
        var csv = service.ExportTrophiesToCSV(winner, (null, null), (null, null),
            [new WineProducer { WineProducerId = "producer", WineyardName = "Vineyard; \"quoted\"" }]);

        csv.ShouldStartWith("Pokal;Vinnummer;Vinnavn;Produsent;Gruppe;Land;Årgang;Total Score;Klassifisering;Loddtrekning\r\n");
        using var reader = CsvReader.FromText(csv, new CsvReaderOptions { Separator = ';' });
        reader.Read().ShouldBe(hasWinner);
        if (hasWinner)
        {
            reader.GetString("Pokal").ShouldBe("Årets Vinbonde");
            reader.GetString("Vinnavn").ShouldBe("'=1+1");
            reader.GetString("Produsent").ShouldBe("Vineyard; \"quoted\"");
            reader.Read().ShouldBeFalse();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FlightCsv_PreservesHeadersAndEscapesFields(bool hasWine)
    {
        var service = new ExportService(FrozenTimeProvider.At(2026, 10, 23));
        var csv = service.ExportFlightList(
            [new Flight { FlightName = "@flight", WineIds = hasWine ? ["wine"] : [] }],
            [new Wine { WineId = "wine", WineNumber = 7, RatingName = "Wine; \"quoted\"" }]);

        csv.ShouldStartWith("Flight;Vinnummer;Vinnavn;Kategori;Gruppe;Klasse\r\n");
        using var reader = CsvReader.FromText(csv, new CsvReaderOptions { Separator = ';' });
        reader.Read().ShouldBe(hasWine);
        if (hasWine)
        {
            reader.GetString("Flight").ShouldBe("'@flight");
            reader.GetString("Vinnavn").ShouldBe("Wine; \"quoted\"");
            reader.GetString("Vinnummer").ShouldBe("7");
            reader.Read().ShouldBeFalse();
        }
    }

    [Fact]
    public void EventCsv_WritesAllSectionsWithEscapingAndHeaderlessMetadata()
    {
        var service = new ExportService(FrozenTimeProvider.At(2026, 10, 23));
        var csv = service.ExportEventData(
            new Event { Name = "=event" },
            [new Wine { WineId = "wine", WineNumber = 7, Name = "+wine" }],
            [new WineRating { WineId = "wine", JudgeId = "@judge", Comment = "\n=comment; \"quoted\"" }],
            [new WineResult { WineId = "wine", Classification = Classification.Gold }],
            []);

        var metadata = csv.Split($"## ARRANGEMENT{Environment.NewLine}")[1]
            .Split($"{Environment.NewLine}## VINER")[0];
        using (var reader = CsvReader.FromText(metadata, new CsvReaderOptions { Separator = ';', HasHeader = false }))
        {
            reader.Read().ShouldBeTrue();
            reader.GetString(0).ShouldBe("Navn");
            reader.GetString(1).ShouldBe("'=event");
        }

        var sections = new[]
        {
            (Title: "VINER", Header: "Vinnummer;Navn;Vurderingsnavn;Produsent;Gruppe;Klasse;Kategori;Årgang;Alkohol%;Land;Vinbonde;Betalt\r\n", Column: "Navn", Value: "'+wine"),
            (Title: "VURDERINGER", Header: "Vinnummer;Dommer;Utseende;Nese;Smak;Total;Kommentar;Dato\r\n", Column: "Kommentar", Value: "'\n=comment; \"quoted\""),
            (Title: "RESULTATER", Header: "Vinnummer;Vinnavn;Total Score;Klassifisering;Antall Vurderinger;Spread;Defekt;Outlier\r\n", Column: "Vinnavn", Value: "'+wine"),
        };
        foreach (var section in sections)
        {
            var start = csv.IndexOf($"## {section.Title}{Environment.NewLine}", StringComparison.Ordinal)
                + $"## {section.Title}{Environment.NewLine}".Length;
            var end = csv.IndexOf($"{Environment.NewLine}## ", start, StringComparison.Ordinal);
            var content = end < 0 ? csv[start..] : csv[start..end];
            content.ShouldStartWith(section.Header);
            using var reader = CsvReader.FromText(content, new CsvReaderOptions { Separator = ';' });
            reader.Read().ShouldBeTrue();
            reader.GetString(section.Column).ShouldBe(section.Value);
            reader.GetString("Vinnummer").ShouldBe("7");
        }
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