using WineApp.Data;
using WineApp.Models;

namespace WineApp.Services;

public class WineEventService : IWineEventService
{
    private readonly IEventRepository _eventRepository;

    public WineEventService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public Task<List<Event>> GetAllEventsAsync() => _eventRepository.GetAllEventsAsync();
    public Task<Event?> GetEventByIdAsync(string id) => _eventRepository.GetEventByIdAsync(id);
    public Task<Event?> GetActiveEventAsync() => _eventRepository.GetActiveEventAsync();
    public Task AddEventAsync(Event evt) => _eventRepository.AddEventAsync(evt);
    public Task UpdateEventAsync(Event evt) => _eventRepository.UpdateEventAsync(evt);
    public Task DeleteEventAsync(string id) => _eventRepository.DeleteEventAsync(id);
}
