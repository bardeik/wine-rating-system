using WineApp.Models;

namespace WineApp.Services;

public interface IAuditRetentionService
{
    Task<List<Event>> GetEventsAsync(CancellationToken cancellationToken = default);
    Task RecordEventEndAsync(string eventId, DateTime endedAtUtc, CancellationToken cancellationToken = default);
    Task SetDisputeHoldAsync(string eventId, string? disputeReference, CancellationToken cancellationToken = default);
    Task<long> RunCleanupAsync(CancellationToken cancellationToken = default);
}