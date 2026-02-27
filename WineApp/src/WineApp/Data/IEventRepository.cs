using WineApp.Models;

namespace WineApp.Data;

public interface IEventRepository
{
    Task<List<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(string id);
    Task<Event?> GetActiveEventAsync();
    Task<Event?> GetEventByYearAsync(int year);
    Task AddEventAsync(Event eventItem);
    Task UpdateEventAsync(Event eventItem);
    Task DeleteEventAsync(string id);
}
