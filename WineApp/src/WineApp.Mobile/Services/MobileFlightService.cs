using System.Net.Http.Json;
using WineApp.Shared.Dtos;
using WineApp.Shared.MobileServices;

namespace WineApp.Mobile.Services;

public class MobileFlightService(HttpClient httpClient) : IMobileFlightService
{
    /// <inheritdoc />
    public async Task<IList<WineDto>> GetMyFlightWinesAsync()
    {
        var wines = await httpClient.GetFromJsonAsync<IList<WineDto>>("/api/mobile/flights/my");
        return wines ?? [];
    }
}
