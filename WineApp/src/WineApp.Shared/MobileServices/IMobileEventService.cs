using WineApp.Shared.Dtos;

namespace WineApp.Shared.MobileServices;

public interface IMobileEventService
{
    Task<EventDto?> GetActiveEventAsync();
}
