using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class EventRepository : IEventRepository
{
    private readonly IMongoCollection<Event> _events;

    public EventRepository(WineMongoDbContext context) =>
        _events = context.Events;

    public async Task<List<Event>> GetAllEventsAsync() =>
        await _events.Find(_ => true).ToListAsync();

    public async Task<Event?> GetEventByIdAsync(string id) =>
        await _events.Find(e => e.EventId == id).FirstOrDefaultAsync();

    public async Task<Event?> GetActiveEventAsync() =>
        await _events.Find(e => e.IsActive).FirstOrDefaultAsync();

    public async Task<Event?> GetEventByYearAsync(int year) =>
        await _events.Find(e => e.Year == year).FirstOrDefaultAsync();

    public async Task AddEventAsync(Event eventItem) =>
        await _events.InsertOneAsync(eventItem);

    public async Task UpdateEventAsync(Event eventItem) =>
        await _events.ReplaceOneAsync(e => e.EventId == eventItem.EventId, eventItem);

    public async Task DeleteEventAsync(string id) =>
        await _events.DeleteOneAsync(e => e.EventId == id);
}
