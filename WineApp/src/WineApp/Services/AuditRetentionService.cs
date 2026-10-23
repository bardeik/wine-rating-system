using System.Text.Json;
using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public sealed class AuditRetentionService(SecurityAccess access, IAuditRetentionStore store,
    ISecurityMutationStore mutations, TimeProvider clock, ILogger<AuditRetentionService> logger) : IAuditRetentionService
{
    public async Task<List<Event>> GetEventsAsync(CancellationToken cancellationToken = default)
    {
        await access.RequireAdminAsync();
        return await store.GetEventsAsync(cancellationToken);
    }

    public async Task RecordEventEndAsync(string eventId, DateTime endedAtUtc, CancellationToken cancellationToken = default)
    {
        var actor = await access.RequireAdminAsync();
        if (endedAtUtc.Kind != DateTimeKind.Utc || endedAtUtc > clock.GetUtcNow().UtcDateTime)
            throw new ArgumentException("Record the actual event end in UTC; future dates are not permitted.", nameof(endedAtUtc));
        await store.WithEventLockAsync(eventId, async () =>
        {
            var before = await RequireEventAsync(eventId, cancellationToken);
            if (endedAtUtc.Year < before.Year)
                throw new ArgumentException("The end date cannot precede the event year.", nameof(endedAtUtc));
            var after = Clone(before);
            after.EndedAtUtc = endedAtUtc;
            after.IsActive = false;
            await mutations.CommitAsync(WineMongoDbContext.EventsCollection, eventId,
                before.SecurityRevision, before, after, actor, "EventEndRecorded");
        }, cancellationToken);
    }

    public async Task SetDisputeHoldAsync(string eventId, string? disputeReference, CancellationToken cancellationToken = default)
    {
        var actor = await access.RequireAdminAsync();
        var reference = disputeReference?.Trim();
        if (reference is not null && (reference.Length is < 1 or > 200 || reference.Any(char.IsControl)))
            throw new ArgumentException("Provide a case reference of 1 to 200 characters, without control characters.", nameof(disputeReference));
        await store.WithEventLockAsync(eventId, async () =>
        {
            var before = await RequireEventAsync(eventId, cancellationToken);
            var after = Clone(before);
            after.AuditDisputeReference = reference;
            await mutations.CommitAsync(WineMongoDbContext.EventsCollection, eventId,
                before.SecurityRevision, before, after, actor, reference is null ? "AuditDisputeHoldReleased" : "AuditDisputeHoldPlaced");
        }, cancellationToken);
    }

    public async Task<long> RunCleanupAsync(CancellationToken cancellationToken = default) =>
        await CleanupAsync(await access.RequireAdminAsync(), cancellationToken);

    internal Task<long> RunScheduledAsync(CancellationToken cancellationToken) => CleanupAsync("system:audit-retention", cancellationToken);

    private async Task<long> CleanupAsync(string actor, CancellationToken cancellationToken)
    {
        long affected = 0;
        foreach (var candidate in await store.GetEventsAsync(cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!AuditRetentionPolicy.CanPurge(candidate, clock.GetUtcNow().UtcDateTime)) continue;
            try
            {
                await store.WithEventLockAsync(candidate.EventId, async () =>
                {
                    var current = await RequireEventAsync(candidate.EventId, cancellationToken);
                    var now = clock.GetUtcNow().UtcDateTime;
                    if (!AuditRetentionPolicy.CanPurge(current, now)) return;
                    var count = await store.RemoveExpiredEntriesAsync(current.EventId,
                        now.AddMonths(-AuditRetentionPolicy.RetentionMonths), cancellationToken);
                    affected += count;
                    if (count > 0)
                    {
                        await mutations.CommitAsync(WineMongoDbContext.EventsCollection, current.EventId,
                            current.SecurityRevision, current, current, actor, "ExpiredAuditHistoryRemoved");
                        logger.LogInformation("Audit retention updated {Count} documents for event {EventId}", count, current.EventId);
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "Audit retention failed or is busy for event {EventId}", candidate.EventId);
                throw;
            }
        }
        return affected;
    }

    private async Task<Event> RequireEventAsync(string eventId, CancellationToken cancellationToken) =>
        await store.GetEventAsync(eventId, cancellationToken) ?? throw new InvalidOperationException("Event not found.");

    private static Event Clone(Event evt) => JsonSerializer.Deserialize<Event>(JsonSerializer.Serialize(evt))!;
}