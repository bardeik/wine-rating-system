using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class WineEventService : IWineEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly SecurityAccess _access;
    private readonly ISecurityMutationStore _mutations;

    public WineEventService(IEventRepository eventRepository, SecurityAccess access, ISecurityMutationStore mutations)
    {
        _eventRepository = eventRepository;
        _access = access;
        _mutations = mutations;
    }

    public Task<List<Event>> GetAllEventsAsync() => _eventRepository.GetAllEventsAsync();
    public Task<Event?> GetEventByIdAsync(string id) => _eventRepository.GetEventByIdAsync(id);
    public Task<Event?> GetActiveEventAsync() => _eventRepository.GetActiveEventAsync();
    public async Task AddEventAsync(Event evt)
    {
        var actor = await _access.RequireAdminAsync();
        evt.ResultsPublished = false;
        evt.PublishedAtUtc = null;
        evt.PublishedByUserId = null;
        evt.IsDeleted = false;
        evt.EndedAtUtc = null;
        evt.AuditDisputeReference = null;
        await _mutations.CommitAsync<Event>(WineMongoDbContext.EventsCollection, evt.EventId,
            null, null, evt, actor, "EventCreated");
        evt.SecurityRevision = 1;
    }

    public async Task UpdateEventAsync(Event evt)
    {
        var actor = await _access.RequireAdminAsync();
        var before = await _eventRepository.GetEventByIdAsync(evt.EventId) ?? throw new InvalidOperationException("Event not found.");
        if (before.SecurityRevision != evt.SecurityRevision) throw new InvalidOperationException("Reload the event before editing.");
        if (before.EndedAtUtc is not null && evt.IsActive)
            throw new InvalidOperationException("A completed event cannot be reactivated.");
        evt.EndedAtUtc = before.EndedAtUtc;
        evt.AuditDisputeReference = before.AuditDisputeReference;
        evt.ResultsPublished = before.ResultsPublished;
        evt.PublishedAtUtc = before.PublishedAtUtc;
        evt.PublishedByUserId = before.PublishedByUserId;
        evt.IsDeleted = false;
        await _mutations.CommitAsync(WineMongoDbContext.EventsCollection, evt.EventId,
            before.SecurityRevision, before, evt, actor, "EventUpdated");
        evt.SecurityRevision = before.SecurityRevision + 1;
    }

    public async Task DeleteEventAsync(string id)
    {
        var actor = await _access.RequireAdminAsync();
        var before = await _eventRepository.GetEventByIdAsync(id) ?? throw new InvalidOperationException("Event not found.");
        var after = System.Text.Json.JsonSerializer.Deserialize<Event>(System.Text.Json.JsonSerializer.Serialize(before))!;
        after.IsDeleted = true;
        after.IsActive = false;
        after.ResultsPublished = false;
        await _mutations.CommitAsync(WineMongoDbContext.EventsCollection, id,
            before.SecurityRevision, before, after, actor, "EventDeleted");
    }
}
