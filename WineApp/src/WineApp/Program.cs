using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using WineApp.Data;
using WineApp.Models;
using WineApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// MongoDB context (domain collections)
builder.Services.AddSingleton<WineMongoDbContext>();

// MongoDB Identity
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "wineapp";

builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
})
.AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(mongoConnectionString, mongoDatabaseName)
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Register repositories
builder.Services.AddSingleton<IWineProducerRepository, WineProducerRepository>();
builder.Services.AddSingleton<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddSingleton<IWineRepository, WineRepository>();
builder.Services.AddSingleton<IEventRepository, EventRepository>();
builder.Services.AddSingleton<IWineResultRepository, WineResultRepository>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();

// Register business logic services
builder.Services.AddSingleton<IClassificationService, ClassificationService>();
builder.Services.AddSingleton<IScoreAggregationService, ScoreAggregationService>();
builder.Services.AddSingleton<IWineNumberService, WineNumberService>();
builder.Services.AddSingleton<ITrophyService, TrophyService>();
builder.Services.AddSingleton<IOutlierDetectionService, OutlierDetectionService>();
builder.Services.AddSingleton<IWineValidationService, WineValidationService>();
builder.Services.AddSingleton<IFlightService, FlightService>();
builder.Services.AddSingleton<IExportService, ExportService>();
builder.Services.AddSingleton<IPdfService, PdfService>();

var app = builder.Build();

// Seed roles, default admin and sample data on startup
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapControllers();

// ============================================
// Minimal API Endpoints for CSV Downloads
// ============================================

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

app.Run();
