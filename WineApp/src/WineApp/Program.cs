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

// MongoDB context (domain collections)
builder.Services.AddSingleton<WineMongoDbContext>();

// Clock abstraction — lets tests substitute a frozen clock
builder.Services.AddSingleton(TimeProvider.System);

// MongoDB Identity
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
var mongoDatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"] ?? "wineapp";

builder.Services.AddIdentity<ApplicationUser, MongoIdentityRole<Guid>>(options =>
{
    // RequireConfirmedAccount is intentionally false: all user accounts are created
    // by an Admin (judges, producers) or by the seeder — there is no public
    // self-registration flow, so email confirmation is unnecessary.
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    // Lockout: after 5 failed attempts, lock for 15 minutes
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
})
.AddMongoDbStores<ApplicationUser, MongoIdentityRole<Guid>, Guid>(mongoConnectionString, mongoDatabaseName)
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
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
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IWineEventService, WineEventService>();
builder.Services.AddScoped<IWineCatalogService, WineCatalogService>();
builder.Services.AddScoped<IWineJudgingService, WineJudgingService>();
builder.Services.AddScoped<CurrentUserState>();
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

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
    await next();
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapDownloadEndpoints();

app.Run();
