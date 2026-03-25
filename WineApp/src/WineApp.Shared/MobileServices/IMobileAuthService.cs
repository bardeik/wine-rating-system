using WineApp.Shared.Dtos;

namespace WineApp.Shared.MobileServices;

public interface IMobileAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<MobileUserDto?> GetCurrentUserAsync();
}
