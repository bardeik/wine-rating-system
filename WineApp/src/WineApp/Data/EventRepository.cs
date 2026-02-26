using MongoDB.Driver;
using WineApp.Models;

namespace WineApp.Data;

public class EventRepository : IEventRepository
{
    private readonly IMongoCollection<Event> _events;

    public EventRepository(WineMongoDbContext context) =>
        _events = context.Events;

    public List<Event> GetAllEvents() => 
        _events.Find(_ => true).ToList();

    public Event? GetEventById(string id) => 
        _events.Find(e => e.EventId == id).FirstOrDefault();

    public Event? GetActiveEvent() => 
        _events.Find(e => e.IsActive).FirstOrDefault();

    public Event? GetEventByYear(int year) => 
        _events.Find(e => e.Year == year).FirstOrDefault();

    public void AddEvent(Event eventItem) => 
        _events.InsertOne(eventItem);

    public void UpdateEvent(Event eventItem) => 
        _events.ReplaceOne(e => e.EventId == eventItem.EventId, eventItem);

    public void DeleteEvent(string id) => 
        _events.DeleteOne(e => e.EventId == id);
}
