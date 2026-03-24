using System.Net.Http.Json;
using WineApp.Shared.Dtos;
using WineApp.Shared.MobileServices;

namespace WineApp.Mobile.Services;

public class MobileWineService(HttpClient httpClient) : IMobileWineService
{
    /// <inheritdoc />
    public async Task<IList<WineDto>> GetWinesForActiveEventAsync()
    {
        var wines = await httpClient.GetFromJsonAsync<IList<WineDto>>("/api/mobile/wines");
        return wines ?? [];
    }

    /// <inheritdoc />
    public async Task<WineDto?> GetWineByIdAsync(string wineId) =>
        await httpClient.GetFromJsonAsync<WineDto>($"/api/mobile/wines/{wineId}");
}
