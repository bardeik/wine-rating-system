using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using WineApp.Data;

namespace WineApp.Services;

public sealed class SecurityAccess(AuthenticationStateProvider authenticationStateProvider,
    IHttpContextAccessor httpContextAccessor, IEventRepository events)
{
    public async Task<ClaimsPrincipal> GetUserAsync() =>
        httpContextAccessor.HttpContext is { } context ? context.User
            : (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

    public async Task<string> RequireAdminAsync()
    {
        var user = await GetUserAsync();
        if (user.Identity?.IsAuthenticated != true || !user.IsInRole("Admin"))
            throw new UnauthorizedAccessException("Administrator access is required.");
        return RequireUserId(user);
    }

    public async Task<string> RequireJudgeAsync()
    {
        var user = await GetUserAsync();
        if (user.Identity?.IsAuthenticated != true || !user.IsInRole("Judge"))
            throw new UnauthorizedAccessException("Judge access is required.");
        return RequireUserId(user);
    }

    public async Task RequireResultsAsync(string eventId)
    {
        var user = await GetUserAsync();
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin")) return;
        if ((await events.GetEventByIdAsync(eventId))?.ResultsPublished != true)
            throw new UnauthorizedAccessException("Results are not published.");
    }

    public static string RequireUserId(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id ? id
            : throw new UnauthorizedAccessException("An authenticated user ID is required.");
}