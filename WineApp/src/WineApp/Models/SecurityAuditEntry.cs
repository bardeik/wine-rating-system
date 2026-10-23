namespace WineApp.Models;

public sealed class SecurityAuditEntry
{
    public string EntryId { get; set; } = Guid.NewGuid().ToString();
    public string ActorUserId { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; }
    public string Action { get; set; } = string.Empty;
    public string TargetId { get; set; } = string.Empty;
    public string Before { get; set; } = string.Empty;
    public string After { get; set; } = string.Empty;
}