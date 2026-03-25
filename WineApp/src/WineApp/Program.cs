using AspNetCore.Identity.MongoDbCore.Models;
using Microsoft.AspNetCore.HttpOverrides;
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

// MongoDB Identity — values come from appsettings.json (local) or environment variables
// (production). Use ConnectionStrings__MongoDB / MongoDbSettings__DatabaseName env vars
// in production (e.g. via `fly secrets set` or your secrets manager).
var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB")
    ?? throw new InvalidOperationException(
           "ConnectionStrings:MongoDB is not configured. " +
           "Add it to appsettings.json or set the ConnectionStrings__MongoDB environment variable.");
var mongoDatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"]
    ?? throw new InvalidOperationException(
           "MongoDbSettings:DatabaseName is not configured. " +
           "Add it to appsettings.json or set the MongoDbSettings__DatabaseName environment variable.");

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
builder.Services.AddScoped<IPaymentService, PaymentService>();
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

// Seed roles, default admin and sample data on startup.
// Wrapped in try-catch so a MongoDB connectivity problem during seeding does not
// crash the process — the app can still start and serve requests.
try
{
    using var scope = app.Services.CreateScope();
    await DatabaseSeeder.SeedAsync(scope.ServiceProvider);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Database seeding failed — the app will continue but some data may be missing.");
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Fly.io (and most reverse proxies) terminate TLS at the edge and forward
// requests internally as plain HTTP. UseForwardedHeaders teaches ASP.NET Core
// to read X-Forwarded-Proto / X-Forwarded-For so it knows the original request
// was HTTPS. KnownNetworks/KnownProxies are cleared so the header is trusted
// regardless of which internal IP the Fly.io proxy originates from.
// UseHttpsRedirection is intentionally omitted: Fly.io enforces HTTPS at the
// edge via force_https = true in fly.toml, so in-app redirection is redundant
// and would cause a redirect loop (the app only ever receives plain HTTP internally).
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
// Clear the default loopback-only restrictions so Fly.io's edge proxy is trusted
forwardedOptions.KnownIPNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

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

// Health check endpoint — used by Fly.io to verify the machine is alive before
// routing traffic to it. Must respond before authentication middleware runs.
app.MapGet("/health", () => Results.Ok("healthy"));

// Temporary diagnostics endpoint — remove once deployment is confirmed working.
app.MapGet("/debug/files", (IWebHostEnvironment env) =>
{
    var contentRoot = env.ContentRootPath ?? "";
    var manifestPath = Path.Combine(contentRoot, "WineApp.staticwebassets.endpoints.json");
    var manifestContent = File.Exists(manifestPath)
        ? File.ReadAllText(manifestPath)
        : "NOT FOUND";
    return Results.Ok(new
    {
        contentRoot,
        webRoot = env.WebRootPath,
        manifestExists = File.Exists(manifestPath),
        manifestContent
    });
});

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapDownloadEndpoints();
app.MapMobileApiEndpoints();

app.Run();
