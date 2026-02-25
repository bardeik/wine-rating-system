using WineApp.Models;

namespace WineApp.Data;

public interface IEventRepository
{
    List<Event> GetAllEvents();
    Event? GetEventById(string id);
    Event? GetActiveEvent();
    Event? GetEventByYear(int year);
    void AddEvent(Event eventItem);
    void UpdateEvent(Event eventItem);
    void DeleteEvent(string id);
}
