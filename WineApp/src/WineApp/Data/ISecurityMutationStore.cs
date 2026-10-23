using WineApp.Models;

namespace WineApp.Data;

public interface ISecurityMutationStore
{
    Task CommitAsync<T>(string collection, string id, int? expectedRevision,
        T? before, T after, string actorId, string action) where T : class;
    Task<List<SecurityAuditEntry>> GetRecentAuditAsync();
}