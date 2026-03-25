namespace WineApp.Mobile.Services;

/// <summary>
/// Persists the auth token using MAUI SecureStorage.
/// Android: backed by Android Keystore. iOS: backed by Keychain.
/// </summary>
public class TokenStore
{
    private const string TokenKey = "auth_token";

    /// <summary>Saves the token to secure storage.</summary>
    public async Task SaveTokenAsync(string token) =>
        await SecureStorage.Default.SetAsync(TokenKey, token);

    /// <summary>Retrieves the stored token, or null if none.</summary>
    public async Task<string?> GetTokenAsync() =>
        await SecureStorage.Default.GetAsync(TokenKey);

    /// <summary>Removes all stored credentials.</summary>
    public Task ClearAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        return Task.CompletedTask;
    }
}
