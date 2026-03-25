using System.Net.Http.Json;
using WineApp.Shared.Dtos;
using WineApp.Shared.MobileServices;

namespace WineApp.Mobile.Services;

public class MobileAuthService(HttpClient httpClient, TokenStore tokenStore) : IMobileAuthService
{
    /// <summary>Validates credentials against the server and stores the returned token.</summary>
    public async Task<bool> LoginAsync(string email, string password)
    {
        var response = await httpClient.PostAsJsonAsync(
            "/api/mobile/auth/login",
            new LoginRequestDto { Email = email, Password = password });

        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        if (result is null) return false;

        await tokenStore.SaveTokenAsync(result.Token);
        return true;
    }

    /// <inheritdoc />
    public async Task LogoutAsync() => await tokenStore.ClearAsync();

    /// <inheritdoc />
    public async Task<string?> GetTokenAsync() => await tokenStore.GetTokenAsync();

    /// <inheritdoc />
    public async Task<MobileUserDto?> GetCurrentUserAsync()
    {
        var token = await tokenStore.GetTokenAsync();
        if (string.IsNullOrEmpty(token)) return null;

        var response = await httpClient.GetAsync("/api/mobile/auth/me");
        if (!response.IsSuccessStatusCode) return null;

        return await response.Content.ReadFromJsonAsync<MobileUserDto>();
    }
}
