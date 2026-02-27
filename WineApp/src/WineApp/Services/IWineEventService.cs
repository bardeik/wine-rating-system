using WineApp.Models;

namespace WineApp.Services;

public interface IWineEventService
{
    Task<List<Event>> GetAllEventsAsync();
    Task<Event?> GetEventByIdAsync(string id);
    Task<Event?> GetActiveEventAsync();
    Task AddEventAsync(Event evt);
    Task UpdateEventAsync(Event evt);
    Task DeleteEventAsync(string id);
}
