using WineApp.Data;
using WineApp.Services;

namespace WineApp.Extensions;

public static class DownloadEndpointExtensions
{
    public static WebApplication MapDownloadEndpoints(this WebApplication app)
    {
        // Download complete results for an event
        app.MapGet("/api/download/results/{eventId}", (
            string eventId,
            IExportService exportService,
            IWineResultRepository resultRepo,
            IWineRepository wineRepo,
            IWineProducerRepository producerRepo,
            IEventRepository eventRepo) =>
        {
            var evt = eventRepo.GetEventById(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var wines = wineRepo.GetAllWines().Where(w => w.EventId == eventId).ToList();
            var results = resultRepo.GetAllWineResults()
                .Where(r => wines.Any(w => w.WineId == r.WineId))
                .ToList();
            var producers = producerRepo.GetAllWineProducers().ToList();

            var csv = exportService.ExportResultsToCSV(results, wines, producers);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Resultater_{evt.Name}_{DateTime.Now:yyyyMMdd}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization();

        // Download trophy winners for an event
        app.MapGet("/api/download/trophies/{eventId}", (
            string eventId,
            IExportService exportService,
            ITrophyService trophyService,
            IWineProducerRepository producerRepo,
            IEventRepository eventRepo) =>
        {
            var evt = eventRepo.GetEventById(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var aaretsVinbonde = trophyService.GetAaretsVinbonde(eventId);
            var bestNorwegian = trophyService.GetBestNorwegianWine(eventId);
            var bestNordic = trophyService.GetBestNordicWine(eventId);
            var producers = producerRepo.GetAllWineProducers().ToList();

            var csv = exportService.ExportTrophiesToCSV(aaretsVinbonde, bestNorwegian, bestNordic, producers);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Pokaler_{evt.Name}_{DateTime.Now:yyyyMMdd}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization();

        // Download complete event archive (wines, ratings, results)
        app.MapGet("/api/download/event/{eventId}", (
            string eventId,
            IExportService exportService,
            IEventRepository eventRepo,
            IWineRepository wineRepo,
            IWineRatingRepository ratingRepo,
            IWineResultRepository resultRepo,
            IWineProducerRepository producerRepo) =>
        {
            var evt = eventRepo.GetEventById(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var wines = wineRepo.GetAllWines().Where(w => w.EventId == eventId).ToList();
            var ratings = ratingRepo.GetAllWineRatings()
                .Where(r => wines.Any(w => w.WineId == r.WineId))
                .ToList();
            var results = resultRepo.GetAllWineResults()
                .Where(r => wines.Any(w => w.WineId == r.WineId))
                .ToList();
            var producers = producerRepo.GetAllWineProducers().ToList();

            var csv = exportService.ExportEventData(evt, wines, ratings, results, producers);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Arkiv_{evt.Name}_{evt.Year}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Download flight list for an event
        app.MapGet("/api/download/flights/{eventId}", (
            string eventId,
            IExportService exportService,
            IFlightService flightService,
            IWineRepository wineRepo,
            IEventRepository eventRepo) =>
        {
            var evt = eventRepo.GetEventById(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var flights = flightService.GetFlightsForEvent(eventId);
            var wines = wineRepo.GetAllWines().ToList();

            var csv = exportService.ExportFlightList(flights, wines);
            var bytes = exportService.GetCSVBytes(csv);
            var fileName = $"Flights_{evt.Name}_{DateTime.Now:yyyyMMdd}.csv";

            return Results.File(bytes, "text/csv", fileName);
        }).RequireAuthorization(policy => policy.RequireRole("Admin"));

        // Download trophy report as PDF
        app.MapGet("/api/download/trophy-pdf/{eventId}", (
            string eventId,
            IPdfService pdfService,
            ITrophyService trophyService,
            IEventRepository eventRepo,
            IWineProducerRepository producerRepo) =>
        {
            var evt = eventRepo.GetEventById(eventId);
            if (evt == null) return Results.NotFound("Event not found");

            var aaretsVinbonde = trophyService.GetAaretsVinbonde(eventId);
            var bestNorwegian = trophyService.GetBestNorwegianWine(eventId);
            var bestNordic = trophyService.GetBestNordicWine(eventId);
            var producers = producerRepo.GetAllWineProducers().ToList();

            var pdf = pdfService.GenerateTrophyReport(evt, aaretsVinbonde, bestNorwegian, bestNordic, producers);
            var fileName = $"Pokaler_{evt.Name}_{DateTime.Now:yyyyMMdd}.pdf";

            return Results.File(pdf, "application/pdf", fileName);
        }).RequireAuthorization();

        return app;
    }
}
