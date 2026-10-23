using WineApp.Models;

namespace WineApp.Services;

public interface ISecurityAdministrationService
{
    Task SetPublicationAsync(string eventId, bool published);
    Task AssignJudgesAsync(string flightId, IReadOnlyCollection<string> judgeIds);
    Task<List<SecurityAuditEntry>> GetRecentAuditAsync();
    Task<List<string>> MigrateLegacyRatingsAsync();
}