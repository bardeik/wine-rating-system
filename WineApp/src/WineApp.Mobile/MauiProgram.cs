using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using WineApp.Mobile.Services;
using WineApp.Shared.MobileServices;

namespace WineApp.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMaui()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Load AppConfig.BaseUrl from the embedded appsettings.json (committed, dev defaults).
        // For production builds, replace or overlay appsettings.json before compiling — see
        // appsettings.Production.example.json for the expected format.
        var appConfig = LoadAppConfig();
        builder.Services.AddSingleton(appConfig);

        // Token store backed by platform SecureStorage
        builder.Services.AddSingleton<TokenStore>();

        // Auth handler injects the Bearer token into every outgoing request
        builder.Services.AddSingleton<AuthTokenHandler>();

        // HttpClient registrations — typed clients bound to service interfaces
        builder.Services.AddHttpClient<IMobileWineService, MobileWineService>((sp, client) =>
        {
            var config = sp.GetRequiredService<AppConfig>();
            client.BaseAddress = new Uri(config.BaseUrl);
        }).AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services.AddHttpClient<IMobileRatingService, MobileRatingService>((sp, client) =>
        {
            var config = sp.GetRequiredService<AppConfig>();
            client.BaseAddress = new Uri(config.BaseUrl);
        }).AddHttpMessageHandler<AuthTokenHandler>();

        // Event service does not require auth (public endpoint)
        builder.Services.AddHttpClient<IMobileEventService, MobileEventService>((sp, client) =>
        {
            var config = sp.GetRequiredService<AppConfig>();
            client.BaseAddress = new Uri(config.BaseUrl);
        });

        builder.Services.AddHttpClient<IMobileFlightService, MobileFlightService>((sp, client) =>
        {
            var config = sp.GetRequiredService<AppConfig>();
            client.BaseAddress = new Uri(config.BaseUrl);
        }).AddHttpMessageHandler<AuthTokenHandler>();

        builder.Services.AddHttpClient<IMobileAuthService, MobileAuthService>((sp, client) =>
        {
            var config = sp.GetRequiredService<AppConfig>();
            client.BaseAddress = new Uri(config.BaseUrl);
        });

        return builder.Build();
    }

    /// <summary>
    /// Reads <see cref="AppConfig"/> from the embedded <c>appsettings.json</c> resource.
    /// Falls back to the class defaults (localhost) if the resource cannot be found or parsed,
    /// so development builds without a settings file still work.
    /// </summary>
    private static AppConfig LoadAppConfig()
    {
        var config = new AppConfig();
        var assembly = Assembly.GetExecutingAssembly();

        // Resource name follows the pattern: {DefaultNamespace}.{RelativeFilePath}
        // The file "appsettings.json" at the project root becomes "WineApp.Mobile.appsettings.json".
        const string resourceName = "WineApp.Mobile.appsettings.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return config;

        try
        {
            using var doc = JsonDocument.Parse(stream);
            if (doc.RootElement.TryGetProperty("AppConfig", out var section) &&
                section.TryGetProperty("BaseUrl", out var baseUrlProp))
            {
                var baseUrl = baseUrlProp.GetString();
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    config.BaseUrl = baseUrl;
            }
        }
        catch (JsonException)
        {
            // Malformed appsettings.json — fall back to class defaults rather than crashing.
        }

        return config;
    }
}
