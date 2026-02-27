using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WineApp.Models;

namespace WineApp.Services;

public class PdfService : IPdfService
{
    private readonly TimeProvider _timeProvider;

    public PdfService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public byte[] GenerateTrophyReport(
        Event eventData,
        (Wine? wine, WineResult? result) aaretsVinbonde,
        (Wine? wine, WineResult? result) bestNorwegian,
        (Wine? wine, WineResult? result) bestNordic,
        List<WineProducer> producers)
    {
        // Configure QuestPDF license (Community license is free)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                // Header
                page.Header()
                    .AlignCenter()
                    .Column(column =>
                    {
                        column.Item().PaddingBottom(10).Text(eventData.Name)
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);

                        column.Item().Text("🏆 Pokalvinnere")
                            .FontSize(18)
                            .SemiBold();

                        column.Item().PaddingTop(5).Text($"År: {eventData.Year}")
                            .FontSize(12)
                            .FontColor(Colors.Grey.Darken2);
                    });

                // Content
                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        // Årets Vinbonde
                        column.Item().PaddingBottom(15).Element(c => RenderTrophy(
                            c,
                            "🏅 Årets Vinbonde",
                            "Høyest score i Gruppe A1 med Vinbonde-status",
                            aaretsVinbonde,
                            producers,
                            Colors.Orange.Medium));

                        // Beste Norske Vin
                        column.Item().PaddingBottom(15).Element(c => RenderTrophy(
                            c,
                            "🇳🇴 Vinskuets beste norske vin",
                            "Høyest score i gruppene A1, B, C, D",
                            bestNorwegian,
                            producers,
                            Colors.Green.Medium));

                        // Beste Nordiske Vin
                        column.Item().Element(c => RenderTrophy(
                            c,
                            "🌍 Vinskuets beste nordiske vin",
                            "Høyest score i gruppene A1 og A2",
                            bestNordic,
                            producers,
                            Colors.Blue.Medium));
                    });

                // Footer
                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Medium))
                    .Text(text =>
                    {
                        text.Span("Norsk Vinskue - ");
                        text.Span($"Generert {_timeProvider.GetLocalNow().DateTime:dd.MM.yyyy HH:mm}");
                    });
            });
        });

        return document.GeneratePdf();
    }

    private void RenderTrophy(
        IContainer container,
        string title,
        string description,
        (Wine? wine, WineResult? result) trophy,
        List<WineProducer> producers,
        string backgroundColor)
    {
        container.Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(15).Column(column =>
        {
            // Title
            column.Item().Background(backgroundColor).Padding(10).Text(title)
                .FontSize(16)
                .Bold()
                .FontColor(Colors.White);

            column.Item().PaddingTop(5).Text(description)
                .FontSize(10)
                .Italic()
                .FontColor(Colors.Grey.Darken1);

            // Winner details
            if (trophy.wine != null && trophy.result != null)
            {
                column.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    // Wine number
                    AddRow(table, "Vinnummer:", trophy.wine.WineNumber?.ToString() ?? "N/A", true);

                    // Wine name
                    AddRow(table, "Vinnavn:", trophy.wine.Name, true);

                    // Producer
                    var producer = producers.FirstOrDefault(p => p.WineProducerId == trophy.wine.WineProducerId);
                    AddRow(table, "Produsent:", producer?.WineyardName ?? "Ukjent");

                    // Vintage
                    AddRow(table, "Årgang:", trophy.wine.Vintage.ToString());

                    // Country (for Nordic wines)
                    if (trophy.wine.Group == WineGroup.A2)
                    {
                        AddRow(table, "Land:", trophy.wine.Country);
                    }

                    // Total score
                    AddRow(table, "Total score:", trophy.result.TotalScore.ToString("F1"), true);

                    // Classification
                    AddRow(table, "Klassifisering:", trophy.result.Classification, true);

                    // Panel averages
                    AddRow(table, "Panel snitt:",
                        $"A: {trophy.result.AverageAppearance:F2} | " +
                        $"B: {trophy.result.AverageNose:F2} | " +
                        $"C: {trophy.result.AverageTaste:F2}");

                    // Number of ratings
                    AddRow(table, "Antall vurderinger:", $"{trophy.result.NumberOfRatings} dommere");

                    // Lottery warning
                    if (trophy.result.RequiresLottery)
                    {
                        table.Cell().ColumnSpan(2).PaddingTop(10).Background(Colors.Orange.Lighten3).Padding(8)
                            .DefaultTextStyle(x => x.FontSize(10))
                            .Text(text =>
                            {
                                text.Span("🎲 Loddtrekning påkrevd! ").Bold();
                                text.Span($"Likt snitt med annen vin. Høyeste enkeltscore: {trophy.result.HighestSingleScore:F1}");
                            });
                    }
                });
            }
            else
            {
                column.Item().PaddingTop(10).AlignCenter().Text("Ingen kvalifisert vin funnet")
                    .FontColor(Colors.Grey.Medium)
                    .Italic();
            }
        });
    }

    private void AddRow(TableDescriptor table, string label, string value, bool bold = false)
    {
        table.Cell().Text(label).SemiBold();
        
        if (bold)
        {
            table.Cell().Text(value).FontSize(11).SemiBold();
        }
        else
        {
            table.Cell().Text(value).FontSize(11);
        }
    }
}
