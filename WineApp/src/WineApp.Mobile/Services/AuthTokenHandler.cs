using System.Net.Http.Headers;

namespace WineApp.Mobile.Services;

/// <summary>
/// Delegating handler that reads the current auth token from <see cref="TokenStore"/>
/// and attaches it as a Bearer token to every outgoing request.
/// </summary>
public class AuthTokenHandler(TokenStore tokenStore) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await tokenStore.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
