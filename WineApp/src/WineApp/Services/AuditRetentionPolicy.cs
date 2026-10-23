using WineApp.Models;

namespace WineApp.Services;

public static class AuditRetentionPolicy
{
    public const int RetentionMonths = 24;

    public static bool CanPurge(Event evt, DateTime utcNow) =>
        utcNow.Kind == DateTimeKind.Utc && !evt.IsActive
        && string.IsNullOrWhiteSpace(evt.AuditDisputeReference)
        && evt.EndedAtUtc is { Kind: DateTimeKind.Utc, Year: <= 9997 } ended
        && ended.AddMonths(RetentionMonths) <= utcNow;
}