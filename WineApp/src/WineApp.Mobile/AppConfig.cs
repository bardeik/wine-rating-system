namespace WineApp.Mobile;

/// <summary>
/// App-level server configuration bound from the embedded <c>appsettings.json</c>
/// (or an optional <c>appsettings.Production.json</c> overlay for production builds).
/// <para>
/// Development — iOS Simulator:    <c>https://localhost:5001</c> or <c>http://localhost:5000</c><br/>
/// Development — Android Emulator: <c>http://10.0.2.2:5000</c> (emulator loopback to host)<br/>
/// Production:                     Full HTTPS URL of the deployed server<br/>
/// </para>
/// <para>
/// To change the URL for a build, edit <c>appsettings.json</c> (or place a
/// <c>appsettings.Production.json</c> overlay alongside it before compiling).
/// </para>
/// </summary>
public class AppConfig
{
    public string BaseUrl { get; set; } = "https://localhost:5001";
}
