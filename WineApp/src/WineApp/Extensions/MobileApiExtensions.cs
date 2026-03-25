using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using WineApp.Data;
using WineApp.Models;
using WineApp.Services;
using static WineApp.Extensions.MobileApiMappings;

namespace WineApp.Extensions;

/// <summary>
/// Minimal API endpoints consumed by the .NET MAUI Blazor Hybrid mobile app.
/// Auth uses IDataProtectionProvider (already wired in ASP.NET Core) — no JWT package needed.
/// Token format: Protect("{userId}|{expiry:O}") → base64url string, 8 h lifetime.
/// </summary>
public static class MobileApiExtensions
{
    private const string ProtectorPurpose = "MobileAuth";
    private const int TokenLifetimeHours = 8;

    public static WebApplication MapMobileApiEndpoints(this WebApplication app)
    {
        var mobile = app.MapGroup("/api/mobile");

        // ------------------------------------------------------------------ //
        // Auth                                                                //
        // ------------------------------------------------------------------ //

        // POST /api/mobile/auth/login
        mobile.MapPost("/auth/login", async (
            LoginRequest body,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IDataProtectionProvider dpProvider,
            TimeProvider timeProvider) =>
        {
            if (string.IsNullOrWhiteSpace(body.Email) || string.IsNullOrWhiteSpace(body.Password))
                return Results.BadRequest("E-post og passord er påkrevd.");

            var user = await userManager.FindByEmailAsync(body.Email);
            if (user is null)
                return Results.Unauthorized();

            var result = await signInManager.CheckPasswordSignInAsync(user, body.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return Results.Unauthorized();

            var roles = (await userManager.GetRolesAsync(user)).ToList();
            var expiry = timeProvider.GetUtcNow().UtcDateTime.AddHours(TokenLifetimeHours);
            var token = CreateToken(dpProvider, user.Id.ToString(), expiry);

            return Results.Ok(new LoginResponse(
                Token: token,
                DisplayName: user.DisplayName ?? user.UserName ?? user.Email ?? string.Empty,
                Roles: roles,
                ExpiresAt: expiry));
        });

        // GET /api/mobile/auth/me
        mobile.MapGet("/auth/me", async (
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            TimeProvider timeProvider) =>
        {
            var user = await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider);
            if (user is null) return Results.Unauthorized();

            var roles = (await userManager.GetRolesAsync(user)).ToList();
            return Results.Ok(new MobileUser(
                UserId: user.Id.ToString(),
                Email: user.Email ?? string.Empty,
                DisplayName: user.DisplayName ?? user.UserName ?? user.Email ?? string.Empty,
                Roles: roles));
        });

        // ------------------------------------------------------------------ //
        // Events                                                              //
        // ------------------------------------------------------------------ //

        // GET /api/mobile/events/active  (no auth — public)
        mobile.MapGet("/events/active", async (IWineEventService eventService) =>
        {
            var evt = await eventService.GetActiveEventAsync();
            if (evt is null) return Results.NotFound();
            return Results.Ok(MapEvent(evt));
        });

        // ------------------------------------------------------------------ //
        // Wines                                                               //
        // ------------------------------------------------------------------ //

        // GET /api/mobile/wines
        mobile.MapGet("/wines", async (
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            IWineCatalogService catalogService,
            IWineEventService eventService,
            TimeProvider timeProvider) =>
        {
            if (await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider) is null)
                return Results.Unauthorized();

            var activeEvent = await eventService.GetActiveEventAsync();
            if (activeEvent is null) return Results.Ok(Array.Empty<WineResponse>());

            var wines = await catalogService.GetAllWinesAsync();
            var eventWines = wines
                .Where(w => w.EventId == activeEvent.EventId)
                .Select(MapWine)
                .ToList();

            return Results.Ok(eventWines);
        });

        // GET /api/mobile/wines/{wineId}
        mobile.MapGet("/wines/{wineId}", async (
            string wineId,
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            IWineCatalogService catalogService,
            TimeProvider timeProvider) =>
        {
            if (await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider) is null)
                return Results.Unauthorized();

            var wine = await catalogService.GetWineByIdAsync(wineId);
            return wine is null ? Results.NotFound() : Results.Ok(MapWine(wine));
        });

        // ------------------------------------------------------------------ //
        // Flights                                                             //
        // ------------------------------------------------------------------ //

        // GET /api/mobile/flights/my  (Judge or Admin only)
        mobile.MapGet("/flights/my", async (
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            IWineEventService eventService,
            IFlightService flightService,
            IWineCatalogService catalogService,
            TimeProvider timeProvider) =>
        {
            var user = await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider);
            if (user is null) return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Judge") && !roles.Contains("Admin"))
                return Results.Forbid();

            var activeEvent = await eventService.GetActiveEventAsync();
            if (activeEvent is null) return Results.Ok(Array.Empty<WineResponse>());

            var flights = await flightService.GetFlightsForEventAsync(activeEvent.EventId);
            var allWineIds = flights.SelectMany(f => f.WineIds).Distinct().ToList();

            var flightWines = (await catalogService.GetWinesByIdsAsync(allWineIds))
                .Select(MapWine)
                .ToList();

            return Results.Ok(flightWines);
        });

        // ------------------------------------------------------------------ //
        // Ratings                                                             //
        // ------------------------------------------------------------------ //

        // GET /api/mobile/ratings/my
        mobile.MapGet("/ratings/my", async (
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            IWineJudgingService judgingService,
            TimeProvider timeProvider) =>
        {
            var user = await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider);
            if (user is null) return Results.Unauthorized();

            var mine = (await judgingService.GetRatingsByJudgeAsync(user.Id.ToString()))
                .Select(MapRating)
                .ToList();

            return Results.Ok(mine);
        });

        // GET /api/mobile/ratings/wine/{wineId}
        mobile.MapGet("/ratings/wine/{wineId}", async (
            string wineId,
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            IWineJudgingService judgingService,
            TimeProvider timeProvider) =>
        {
            var user = await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider);
            if (user is null) return Results.Unauthorized();

            var rating = await judgingService.GetRatingByWineAndJudgeAsync(wineId, user.Id.ToString());
            return rating is null ? Results.NotFound() : Results.Ok(MapRating(rating));
        });

        // POST /api/mobile/ratings  (Judge or Admin only)
        mobile.MapPost("/ratings", async (
            RatingRequest body,
            HttpContext ctx,
            UserManager<ApplicationUser> userManager,
            IDataProtectionProvider dpProvider,
            IWineJudgingService judgingService,
            TimeProvider timeProvider) =>
        {
            var user = await ValidateTokenAsync(ctx, userManager, dpProvider, timeProvider);
            if (user is null) return Results.Unauthorized();

            var roles = await userManager.GetRolesAsync(user);
            if (!roles.Contains("Judge") && !roles.Contains("Admin"))
                return Results.Forbid();

            if (string.IsNullOrWhiteSpace(body.WineId))
                return Results.BadRequest("WineId er påkrevd.");

            // Validate score ranges
            if (body.Appearance < 0 || body.Appearance > 3)
                return Results.BadRequest("Utseende må være mellom 0 og 3.");
            if (body.Nose < 0 || body.Nose > 4)
                return Results.BadRequest("Nese må være mellom 0 og 4.");
            if (body.Taste < 0 || body.Taste > 13)
                return Results.BadRequest("Smak må være mellom 0 og 13.");

            var judgeId = user.Id.ToString();
            var existing = await judgingService.GetRatingByWineAndJudgeAsync(body.WineId, judgeId);
            var now = timeProvider.GetUtcNow().UtcDateTime;

            if (existing is not null)
            {
                existing.Appearance = body.Appearance;
                existing.Nose = body.Nose;
                existing.Taste = body.Taste;
                existing.Comment = body.Comment ?? string.Empty;
                existing.RatingDate = now;
                await judgingService.UpdateWineRatingAsync(existing);
                return Results.Ok(MapRating(existing));
            }

            var newRating = new WineRating
            {
                JudgeId = judgeId,
                WineId = body.WineId,
                Appearance = body.Appearance,
                Nose = body.Nose,
                Taste = body.Taste,
                Comment = body.Comment ?? string.Empty,
                RatingDate = now
            };
            await judgingService.AddWineRatingAsync(newRating);
            return Results.Created($"/api/mobile/ratings/wine/{body.WineId}", MapRating(newRating));
        });

        return app;
    }

    private static string CreateToken(IDataProtectionProvider dpProvider, string userId, DateTime expiry)
    {
        var protector = dpProvider.CreateProtector(ProtectorPurpose);
        var plainText = $"{userId}|{expiry:O}";
        var protectedBytes = protector.Protect(Encoding.UTF8.GetBytes(plainText));
        return Base64UrlEncode(protectedBytes);
    }

    private static async Task<ApplicationUser?> ValidateTokenAsync(
        HttpContext ctx,
        UserManager<ApplicationUser> userManager,
        IDataProtectionProvider dpProvider,
        TimeProvider timeProvider)
    {
        var authHeader = ctx.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        var token = authHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var protector = dpProvider.CreateProtector(ProtectorPurpose);
            var protectedBytes = Base64UrlDecode(token);
            var plainBytes = protector.Unprotect(protectedBytes);
            var plain = Encoding.UTF8.GetString(plainBytes);

            var separatorIndex = plain.IndexOf('|');
            if (separatorIndex < 0) return null;

            var userId = plain[..separatorIndex];
            var expiryString = plain[(separatorIndex + 1)..];

            if (!DateTime.TryParse(expiryString, null,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var expiry))
                return null;

            if (timeProvider.GetUtcNow().UtcDateTime > expiry)
                return null;

            return await userManager.FindByIdAsync(userId);
        }
        catch (CryptographicException)
        {
            // Expected when the token has been tampered with or is invalid
            return null;
        }
        catch (Exception ex)
        {
            // Unexpected failure — log it for diagnostics without leaking details to the caller
            var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>()
                .CreateLogger(nameof(MobileApiExtensions));
            logger.LogWarning(ex, "Unexpected error while validating mobile auth token");
            return null;
        }
    }

    // -------------------------------------------------------------------------
    // Request-only record types (not shared with the response side)
    // -------------------------------------------------------------------------

    private sealed record LoginRequest(string Email, string Password);

    private sealed record LoginResponse(
        string Token,
        string DisplayName,
        IList<string> Roles,
        DateTime ExpiresAt);

    private sealed record MobileUser(
        string UserId,
        string Email,
        string DisplayName,
        IList<string> Roles);

    private sealed record RatingRequest(
        string? WineRatingId,
        decimal Appearance,
        decimal Nose,
        decimal Taste,
        string? Comment,
        string WineId);
}
