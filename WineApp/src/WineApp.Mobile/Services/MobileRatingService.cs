using System.Net.Http.Json;
using WineApp.Shared.Dtos;
using WineApp.Shared.MobileServices;

namespace WineApp.Mobile.Services;

public class MobileRatingService(HttpClient httpClient) : IMobileRatingService
{
    /// <inheritdoc />
    public async Task<IList<WineRatingDto>> GetMyRatingsAsync()
    {
        var ratings = await httpClient.GetFromJsonAsync<IList<WineRatingDto>>("/api/mobile/ratings/my");
        return ratings ?? [];
    }

    /// <inheritdoc />
    public async Task<WineRatingDto?> GetRatingForWineAsync(string wineId, string judgeId) =>
        await httpClient.GetFromJsonAsync<WineRatingDto?>($"/api/mobile/ratings/wine/{wineId}");

    /// <inheritdoc />
    public async Task<bool> SaveRatingAsync(WineRatingDto rating)
    {
        var response = await httpClient.PostAsJsonAsync("/api/mobile/ratings", rating);
        return response.IsSuccessStatusCode;
    }
}
