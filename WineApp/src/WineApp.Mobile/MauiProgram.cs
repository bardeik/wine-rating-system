using Microsoft.Extensions.Logging;
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

        // App configuration — update BaseUrl to point to your deployed server.
        // Development (iOS Simulator): use "http://localhost:5000" or "https://localhost:5001"
        // Development (Android Emulator): use "http://10.0.2.2:5000" (emulator loopback to host)
        // Production: use the full HTTPS URL of your deployed server (e.g. https://your-app.fly.dev)
        builder.Services.AddSingleton(new AppConfig
        {
            BaseUrl = "https://localhost:5001"
        });

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
}
