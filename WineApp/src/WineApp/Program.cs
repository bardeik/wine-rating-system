using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using WineApp.Data;
using WineApp.Extensions;
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

// Register repositories - Scoped so each Blazor Server circuit (user session) gets
// its own instance, preventing shared mutable state across concurrent users.
builder.Services.AddScoped<IWineProducerRepository, WineProducerRepository>();
builder.Services.AddScoped<IWineRatingRepository, WineRatingRepository>();
builder.Services.AddScoped<IWineRepository, WineRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IWineResultRepository, WineResultRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IFlightRepository, FlightRepository>();

// Register business logic services - Scoped for the same reason; services that
// inject Scoped repositories must themselves be Scoped or transient.
builder.Services.AddScoped<IClassificationService, ClassificationService>();
builder.Services.AddScoped<IScoreAggregationService, ScoreAggregationService>();
builder.Services.AddScoped<IWineNumberService, WineNumberService>();
builder.Services.AddScoped<ITrophyService, TrophyService>();
builder.Services.AddScoped<IOutlierDetectionService, OutlierDetectionService>();
builder.Services.AddScoped<IWineValidationService, WineValidationService>();
builder.Services.AddScoped<IFlightService, FlightService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddScoped<IPdfService, PdfService>();

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
app.MapDownloadEndpoints();

app.Run();
