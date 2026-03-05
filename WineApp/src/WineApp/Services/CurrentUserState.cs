using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace WineApp.Services;

/// <summary>
/// Scoped service that holds the current authenticated user's resolved identity for a Blazor Server circuit.
/// Call <see cref="EnsureInitializedAsync"/> in <c>OnInitializedAsync</c> before accessing user properties.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="EnsureInitializedAsync"/> always re-reads from the
/// <see cref="AuthenticationStateProvider"/> so that role changes made by an admin
/// are reflected on the next component initialisation — no stale cache.
/// </para>
/// <para>
/// Use <see cref="ForceRefreshAsync"/> to explicitly invalidate and reload the state
/// from within an already-initialised component (e.g. after a role mutation on the current page).
/// </para>
/// </remarks>
public class CurrentUserState
{
    private readonly AuthenticationStateProvider _authStateProvider;

    public string UserId        { get; private set; } = string.Empty;
    public string UserName      { get; private set; } = string.Empty;
    public bool   IsAdmin        { get; private set; }
    public bool   IsJudge        { get; private set; }
    public bool   IsWineProducer { get; private set; }
    public bool   IsViewer       { get; private set; }
    public bool   IsAuthenticated { get; private set; }

    public CurrentUserState(AuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    /// <summary>
    /// Reads the current authentication state and refreshes all role properties.
    /// Safe to call multiple times — each call reflects the latest auth state.
    /// </summary>
    public async Task EnsureInitializedAsync()
    {
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
    }

    /// <summary>
    /// Explicitly refreshes the user state. Equivalent to <see cref="EnsureInitializedAsync"/>.
    /// Provided as a named alias for call sites that want to make the intent explicit,
    /// e.g. after an admin performs a role mutation on the current page.
    /// </summary>
    public Task ForceRefreshAsync() => EnsureInitializedAsync();
}
