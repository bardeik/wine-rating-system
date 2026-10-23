using WineApp.Data;
using WineApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WineApp.Services;

public class WineJudgingService : IWineJudgingService
{
    private readonly IWineRatingRepository _wineRatingRepository;
    private readonly IWineResultRepository _wineResultRepository;
    private readonly IWineRepository _wines;
    private readonly IFlightRepository _flights;
    private readonly IEventRepository _events;
    private readonly SecurityAccess _access;
    private readonly ISecurityMutationStore _mutations;
    private readonly TimeProvider _clock;

    public WineJudgingService(IWineRatingRepository wineRatingRepository, IWineResultRepository wineResultRepository,
        IWineRepository wines, IFlightRepository flights, IEventRepository events,
        SecurityAccess access, ISecurityMutationStore mutations, TimeProvider clock)
    {
        _wineRatingRepository = wineRatingRepository;
        _wineResultRepository = wineResultRepository;
        _wines = wines;
        _flights = flights;
        _events = events;
        _access = access;
        _mutations = mutations;
        _clock = clock;
    }

    public async Task<IList<WineRating>> GetAllWineRatingsAsync()
    {
        var user = await _access.GetUserAsync();
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin"))
            return await _wineRatingRepository.GetAllWineRatingsAsync();
        var judgeId = await _access.RequireJudgeAsync();
        var active = await _events.GetActiveEventAsync();
        if (active is null) return [];
        var flights = (await _flights.GetFlightsForEventAsync(active.EventId))
            .Where(f => f.AssignedJudgeIds.Contains(judgeId)).ToList();
        var eligible = (await _wines.GetAllWinesAsync())
            .Where(w => w.EventId == active.EventId && w.IsPaid && w.WineNumber.HasValue)
            .Select(w => w.WineId).ToHashSet();
        return (await _wineRatingRepository.GetAllWineRatingsAsync())
            .Where(r => r.JudgeId == judgeId && eligible.Contains(r.WineId)
                && flights.Any(f => f.FlightId == r.FlightId && f.WineIds.Contains(r.WineId))).ToList();
    }

    public async Task<WineRating?> GetWineRatingByIdAsync(string id) =>
        (await GetAllWineRatingsAsync()).FirstOrDefault(r => r.WineRatingId == id);

    public Task AddWineRatingAsync(WineRating rating) => SaveRatingAsync(rating, false);
    public Task UpdateWineRatingAsync(WineRating rating) => SaveRatingAsync(rating, true);

    private async Task SaveRatingAsync(WineRating rating, bool updating)
    {
        var user = await _access.GetUserAsync();
        var isAdmin = user.Identity?.IsAuthenticated == true && user.IsInRole("Admin");
        var actor = isAdmin ? await _access.RequireAdminAsync() : await _access.RequireJudgeAsync();
        var ratingId = !updating && !isAdmin
            ? new MongoDB.Bson.ObjectId(System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes($"judge-score-v1|{actor}|{rating.WineId}"))[..12]).ToString()
            : rating.WineRatingId;
        var before = await _wineRatingRepository.GetWineRatingByIdAsync(ratingId);
        if (updating && (before is null || before.SecurityRevision != rating.SecurityRevision))
            throw new InvalidOperationException("Reload the rating before editing.");
        if (!updating && before is not null) throw new InvalidOperationException("Rating already exists.");
        var after = JsonSerializer.Deserialize<WineRating>(JsonSerializer.Serialize(rating))!;
        after.WineRatingId = ratingId;
        after.IsDeleted = false;
        if (!isAdmin)
        {
            if (before is not null && (before.JudgeId != actor || before.WineId != after.WineId || before.FlightId != after.FlightId))
                throw new UnauthorizedAccessException("This rating does not belong to you.");
            await RequireEligibleAsync(after, actor);
            if (!updating && (await _wineRatingRepository.GetAllWineRatingsAsync())
                .Any(existing => existing.WineId == after.WineId && existing.JudgeId == actor))
                throw new InvalidOperationException("A score already exists for this judge and wine. Reload the page.");
            after.JudgeId = actor;
        }
        else if (!Guid.TryParse(after.JudgeId, out _))
            throw new ValidationException("Use the judge's immutable user ID.");
        Validator.ValidateObject(after, new ValidationContext(after), true);
        after.RatingDate = _clock.GetUtcNow().UtcDateTime;
        await _mutations.CommitAsync(WineMongoDbContext.WineRatingsCollection, after.WineRatingId,
            before?.SecurityRevision, before, after, actor, updating ? "ScoreUpdated" : "ScoreCreated");
        rating.WineRatingId = after.WineRatingId;
        rating.JudgeId = after.JudgeId;
        rating.SecurityRevision = (before?.SecurityRevision ?? 0) + 1;
        rating.RatingDate = after.RatingDate;
    }

    private async Task RequireEligibleAsync(WineRating rating, string actor)
    {
        var flight = rating.FlightId is null ? null : await _flights.GetFlightByIdAsync(rating.FlightId);
        var wine = await _wines.GetWineByIdAsync(rating.WineId);
        var active = await _events.GetActiveEventAsync();
        if (flight is null || active is null || wine is null || !wine.IsPaid || !wine.WineNumber.HasValue
            || flight.EventId != active.EventId || wine.EventId != active.EventId
            || !flight.AssignedJudgeIds.Contains(actor) || !flight.WineIds.Contains(rating.WineId))
            throw new UnauthorizedAccessException("The wine is not eligible for your assigned flight.");
    }

    public async Task DeleteWineRatingAsync(string id)
    {
        var actor = await _access.RequireAdminAsync();
        var before = await _wineRatingRepository.GetWineRatingByIdAsync(id)
            ?? throw new InvalidOperationException("Rating not found.");
        var after = JsonSerializer.Deserialize<WineRating>(JsonSerializer.Serialize(before))!;
        after.IsDeleted = true;
        await _mutations.CommitAsync(WineMongoDbContext.WineRatingsCollection, id,
            before.SecurityRevision, before, after, actor, "ScoreDeleted");
    }

    public async Task<List<WineResult>> GetAllWineResultsAsync()
    {
        var user = await _access.GetUserAsync();
        var results = await _wineResultRepository.GetAllWineResultsAsync();
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin")) return results;
        var published = (await _events.GetAllEventsAsync()).Where(e => e.ResultsPublished).Select(e => e.EventId).ToHashSet();
        var wineIds = (await _wines.GetAllWinesAsync()).Where(w => w.EventId != null && published.Contains(w.EventId))
            .Select(w => w.WineId).ToHashSet();
        return results.Where(r => wineIds.Contains(r.WineId)).ToList();
    }

    public async Task<WineResult?> GetWineResultByIdAsync(string id) =>
        (await GetAllWineResultsAsync()).FirstOrDefault(r => r.WineResultId == id);
    public async Task<WineResult?> GetWineResultByWineIdAsync(string wineId) =>
        (await GetAllWineResultsAsync()).FirstOrDefault(r => r.WineId == wineId);
    public async Task AddWineResultAsync(WineResult result)
    {
        await _access.RequireAdminAsync();
        await _wineResultRepository.AddWineResultAsync(result);
    }
    public async Task UpdateWineResultAsync(WineResult result)
    {
        await _access.RequireAdminAsync();
        await _wineResultRepository.UpdateWineResultAsync(result);
    }
}
