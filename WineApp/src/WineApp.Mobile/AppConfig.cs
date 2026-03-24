namespace WineApp.Mobile;

/// <summary>
/// App-level server configuration.
/// Development — iOS Simulator:    http://localhost:5000 or https://localhost:5001
/// Development — Android Emulator: http://10.0.2.2:5000  (emulator loopback address)
/// Production:                     full HTTPS URL of the deployed server
/// Override by injecting a new <see cref="AppConfig"/> in MauiProgram.cs before building.
/// </summary>
public class AppConfig
{
    public string BaseUrl { get; set; } = "https://localhost:5001";
}
