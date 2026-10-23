using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public sealed class SecurityAdministrationService(SecurityAccess access, ISecurityMutationStore mutations,
    IEventRepository events, IFlightRepository flights, IWineRatingRepository ratings,
    IWineRepository wines, UserManager<ApplicationUser> users, TimeProvider clock) : ISecurityAdministrationService
{
    public async Task SetPublicationAsync(string eventId, bool published)
    {
        var actor = await access.RequireAdminAsync();
        var before = await events.GetEventByIdAsync(eventId) ?? throw new InvalidOperationException("Event not found.");
        var after = Clone(before);
        after.ResultsPublished = published;
        after.PublishedAtUtc = published ? clock.GetUtcNow().UtcDateTime : null;
        after.PublishedByUserId = published ? actor : null;
        await mutations.CommitAsync(WineMongoDbContext.EventsCollection, eventId,
            before.SecurityRevision, before, after, actor, published ? "ResultsPublished" : "ResultsUnpublished");
    }

    public async Task AssignJudgesAsync(string flightId, IReadOnlyCollection<string> judgeIds)
    {
        var actor = await access.RequireAdminAsync();
        var allowed = (await users.GetUsersInRoleAsync("Judge")).Select(user => user.Id.ToString()).ToHashSet();
        if (judgeIds.Any(id => !allowed.Contains(id))) throw new ArgumentException("Only current judges can be assigned.");
        var before = await flights.GetFlightByIdAsync(flightId) ?? throw new InvalidOperationException("Flight not found.");
        var after = Clone(before);
        after.AssignedJudgeIds = judgeIds.Distinct().Order().ToList();
        await mutations.CommitAsync(WineMongoDbContext.FlightsCollection, flightId,
            before.SecurityRevision, before, after, actor, "FlightAssignmentsChanged");
    }

    public async Task<List<SecurityAuditEntry>> GetRecentAuditAsync()
    {
        await access.RequireAdminAsync();
        return await mutations.GetRecentAuditAsync();
    }

    public async Task<List<string>> MigrateLegacyRatingsAsync()
    {
        var actor = await access.RequireAdminAsync();
        var judges = await users.GetUsersInRoleAsync("Judge");
        var allWines = await wines.GetAllWinesAsync();
        var reviewIds = new List<string>();
        foreach (var before in await ratings.GetAllWineRatingsAsync())
        {
            if (Guid.TryParse(before.JudgeId, out _) && before.FlightId is not null) continue;
            var candidates = judges.Where(user => new[] { user.Id.ToString(), user.UserName, user.Email, user.DisplayName }
                .Any(name => string.Equals(name, before.JudgeId, StringComparison.OrdinalIgnoreCase))).ToList();
            var wine = allWines.FirstOrDefault(item => item.WineId == before.WineId);
            var candidateFlights = wine?.EventId is null || candidates.Count != 1 ? []
                : (await flights.GetFlightsForEventAsync(wine.EventId))
                    .Where(flight => flight.WineIds.Contains(wine.WineId)
                        && flight.AssignedJudgeIds.Contains(candidates[0].Id.ToString())).ToList();
            if (candidates.Count != 1 || candidateFlights.Count != 1)
            {
                reviewIds.Add(before.WineRatingId);
                continue;
            }
            var after = Clone(before);
            after.JudgeId = candidates[0].Id.ToString();
            after.FlightId = candidateFlights[0].FlightId;
            await mutations.CommitAsync(WineMongoDbContext.WineRatingsCollection, before.WineRatingId,
                before.SecurityRevision, before, after, actor, "LegacyJudgeIdentityMigrated");
        }
        return reviewIds;
    }

    private static T Clone<T>(T value) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
}