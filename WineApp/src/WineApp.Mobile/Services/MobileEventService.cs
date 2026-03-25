using System.Net.Http.Json;
using WineApp.Shared.Dtos;
using WineApp.Shared.MobileServices;

namespace WineApp.Mobile.Services;

public class MobileEventService(HttpClient httpClient) : IMobileEventService
{
    /// <inheritdoc />
    public async Task<EventDto?> GetActiveEventAsync() =>
        await httpClient.GetFromJsonAsync<EventDto>("/api/mobile/events/active");
}
