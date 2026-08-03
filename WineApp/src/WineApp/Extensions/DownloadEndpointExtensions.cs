using WineApp.Data;
using WineApp.Services;
using System.Text;

namespace WineApp.Extensions;

public static class DownloadEndpointExtensions
{
    public static WebApplication MapDownloadEndpoints(this WebApplication app)
    {
        // Download complete results for an event
        app.MapGet("/api/download/results/{eventId}", async (
            string eventId,
            IExportService exportService,
            IWineResultRepository resultRepo,
            IWineRepository wineRepo,
            IWineProducerRepository producerRepo,
            IEventRepository eventRepo) =>
        {
            var evt = await eventRepo.GetEventByIdAsync(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var allWines = await wineRepo.GetAllWinesAsync();
            var wines = allWines.Where(w => w.EventId == eventId).ToList();
            var allResults = await resultRepo.GetAllWineResultsAsync();
            var results = allResults
                .Where(r => wines.Any(w => w.WineId == r.WineId))
                .ToList();
            var producers = (await producerRepo.GetAllWineProducersAsync()).ToList();

            var csv = exportService.ExportResultsToCSV(results, wines, producers);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Resultater_{ToSafeFileSegment(evt.Name)}_{DateTime.Now:yyyyMMdd}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Viewer"));

        // Download trophy winners for an event
        app.MapGet("/api/download/trophies/{eventId}", async (
            string eventId,
            IExportService exportService,
            ITrophyService trophyService,
            IWineProducerRepository producerRepo,
            IEventRepository eventRepo) =>
        {
            var evt = await eventRepo.GetEventByIdAsync(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var aaretsVinbonde = await trophyService.GetAaretsVinbondeAsync(eventId);
            var bestNorwegian = await trophyService.GetBestNorwegianWineAsync(eventId);
            var bestNordic = await trophyService.GetBestNordicWineAsync(eventId);
            var producers = (await producerRepo.GetAllWineProducersAsync()).ToList();

            var csv = exportService.ExportTrophiesToCSV(aaretsVinbonde, bestNorwegian, bestNordic, producers);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Pokaler_{ToSafeFileSegment(evt.Name)}_{DateTime.Now:yyyyMMdd}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Viewer"));

        // Download complete event archive (wines, ratings, results)
        app.MapGet("/api/download/event/{eventId}", async (
            string eventId,
            IExportService exportService,
            IEventRepository eventRepo,
            IWineRepository wineRepo,
            IWineRatingRepository ratingRepo,
            IWineResultRepository resultRepo,
            IWineProducerRepository producerRepo) =>
        {
            var evt = await eventRepo.GetEventByIdAsync(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var allWines = await wineRepo.GetAllWinesAsync();
            var wines = allWines.Where(w => w.EventId == eventId).ToList();
            var allRatings = await ratingRepo.GetAllWineRatingsAsync();
            var ratings = allRatings
                .Where(r => wines.Any(w => w.WineId == r.WineId))
                .ToList();
            var allResults = await resultRepo.GetAllWineResultsAsync();
            var results = allResults
                .Where(r => wines.Any(w => w.WineId == r.WineId))
                .ToList();
            var producers = (await producerRepo.GetAllWineProducersAsync()).ToList();

            var csv = exportService.ExportEventData(evt, wines, ratings, results, producers);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Arkiv_{ToSafeFileSegment(evt.Name)}_{evt.Year}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Download flight list for an event
        app.MapGet("/api/download/flights/{eventId}", async (
            string eventId,
            IExportService exportService,
            IFlightService flightService,
            IWineRepository wineRepo,
            IEventRepository eventRepo) =>
        {
            var evt = await eventRepo.GetEventByIdAsync(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var flights = await flightService.GetFlightsForEventAsync(eventId);
            var wines = (await wineRepo.GetAllWinesAsync()).ToList();

            var csv = exportService.ExportFlightList(flights, wines);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Flights_{ToSafeFileSegment(evt.Name)}_{DateTime.Now:yyyyMMdd}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Download trophy report as PDF
        app.MapGet("/api/download/trophy-pdf/{eventId}", async (
            string eventId,
            IPdfService pdfService,
            ITrophyService trophyService,
            IEventRepository eventRepo,
            IWineProducerRepository producerRepo) =>
        {
            var evt = await eventRepo.GetEventByIdAsync(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var aaretsVinbonde = await trophyService.GetAaretsVinbondeAsync(eventId);
            var bestNorwegian = await trophyService.GetBestNorwegianWineAsync(eventId);
            var bestNordic = await trophyService.GetBestNordicWineAsync(eventId);
            var producers = (await producerRepo.GetAllWineProducersAsync()).ToList();

            var pdf = pdfService.GenerateTrophyReport(evt, aaretsVinbonde, bestNorwegian, bestNordic, producers);
            var fileName = $"Pokaler_{ToSafeFileSegment(evt.Name)}_{DateTime.Now:yyyyMMdd}.pdf";

            return Results.File(pdf, "application/pdf", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin", "Viewer"));

        return app;
    }

    private static string ToSafeFileSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "fil";
        }

        var cleaned = value.Trim();
        var builder = new StringBuilder(cleaned.Length);

        foreach (var ch in cleaned)
        {
            if (char.IsLetterOrDigit(ch) || ch == '-' || ch == '_')
            {
                builder.Append(ch);
                continue;
            }

            if (char.IsWhiteSpace(ch) || ch == '.' || ch == ',')
            {
                builder.Append('_');
            }
        }

        var safe = builder.ToString().Trim('_');
        if (safe.Length == 0)
        {
            return "fil";
        }

        return safe.Length > 80 ? safe[..80] : safe;
    }
}
