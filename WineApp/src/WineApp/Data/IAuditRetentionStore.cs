using WineApp.Models;

namespace WineApp.Data;

public interface IAuditRetentionStore
{
    Task<List<Event>> GetEventsAsync(CancellationToken cancellationToken = default);
    Task<Event?> GetEventAsync(string eventId, CancellationToken cancellationToken = default);
    Task WithEventLockAsync(string eventId, Func<Task> operation, CancellationToken cancellationToken = default);
    Task<long> RemoveExpiredEntriesAsync(string eventId, DateTime cutoffUtc, CancellationToken cancellationToken = default);
}