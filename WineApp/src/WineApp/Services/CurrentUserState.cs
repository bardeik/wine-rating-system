using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace WineApp.Services;

/// <summary>
/// Scoped service that holds the current authenticated user's state.
/// Call <see cref="EnsureInitializedAsync"/> once in OnInitializedAsync before
/// accessing user properties.
/// </summary>
public class CurrentUserState
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private bool _initialized;

    public string UserId       { get; private set; } = string.Empty;
    public string UserName     { get; private set; } = string.Empty;
    public bool   IsAdmin        { get; private set; }
    public bool   IsJudge        { get; private set; }
    public bool   IsWineProducer { get; private set; }
    public bool   IsViewer       { get; private set; }
    public bool   IsAuthenticated { get; private set; }

    public CurrentUserState(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    /// <summary>Idempotent — safe to call multiple times; initializes only once per circuit.</summary>
    public async Task EnsureInitializedAsync()
    {
        if (_initialized) return;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        IsAuthenticated  = user.Identity?.IsAuthenticated ?? false;
        UserId           = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        UserName         = user.FindFirst(ClaimTypes.Name)?.Value
                        ?? user.Identity?.Name
                        ?? string.Empty;
        IsAdmin        = user.IsInRole("Admin");
        IsJudge        = user.IsInRole("Judge");
        IsWineProducer = user.IsInRole("WineProducer");
        IsViewer       = user.IsInRole("Viewer");

        _initialized = true;
    }
}
