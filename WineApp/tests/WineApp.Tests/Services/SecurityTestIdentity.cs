using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;

namespace WineApp.Tests.Services;

internal static class SecurityTestIdentity
{
    public const string UserId = "11111111-1111-1111-1111-111111111111";

    public static SecurityAccess Create(IEventRepository? events = null, params string[] roles)
    {
        var identity = new ClaimsIdentity(roles.Length == 0 ? null : "Test");
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, UserId));
        foreach (var role in roles) identity.AddClaim(new Claim(ClaimTypes.Role, role));
        var provider = new Mock<AuthenticationStateProvider>();
        provider.Setup(value => value.GetAuthenticationStateAsync())
            .ReturnsAsync(new AuthenticationState(new ClaimsPrincipal(identity)));
        return new SecurityAccess(provider.Object, new HttpContextAccessor(), events ?? Mock.Of<IEventRepository>());
    }

    public static SecurityAccess Admin() => Create(null, "Admin");
}