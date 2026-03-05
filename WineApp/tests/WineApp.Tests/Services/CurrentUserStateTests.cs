using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace WineApp.Tests.Services;

public class CurrentUserStateTests
{
    // -----------------------------------------------------------------------
    //  Tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EnsureInitializedAsync_AuthenticatedAdmin_SetsPropertiesCorrectly()
    {
        var principal = BuildUser("user-1", "adminuser", "Admin", "Viewer");
        var provider  = new TestAuthStateProvider(principal);
        var sut       = new CurrentUserState(provider);

        await sut.EnsureInitializedAsync();

        sut.IsAuthenticated.ShouldBeTrue();
        sut.UserId.ShouldBe("user-1");
        sut.UserName.ShouldBe("adminuser");
        sut.IsAdmin.ShouldBeTrue();
        sut.IsJudge.ShouldBeFalse();
        sut.IsWineProducer.ShouldBeFalse();
        sut.IsViewer.ShouldBeTrue();
    }

    [Fact]
    public async Task EnsureInitializedAsync_Judge_SetsRolesCorrectly()
    {
        var principal = BuildUser("user-2", "judgeuser", "Judge", "Viewer");
        var provider  = new TestAuthStateProvider(principal);
        var sut       = new CurrentUserState(provider);

        await sut.EnsureInitializedAsync();

        sut.IsJudge.ShouldBeTrue();
        sut.IsAdmin.ShouldBeFalse();
        sut.IsWineProducer.ShouldBeFalse();
    }

    [Fact]
    public async Task EnsureInitializedAsync_UnauthenticatedUser_SetsIsAuthenticatedFalse()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        var provider  = new TestAuthStateProvider(anonymous);
        var sut       = new CurrentUserState(provider);

        await sut.EnsureInitializedAsync();

        sut.IsAuthenticated.ShouldBeFalse();
        sut.UserId.ShouldBe(string.Empty);
        sut.UserName.ShouldBe(string.Empty);
    }

    [Fact]
    public async Task EnsureInitializedAsync_CalledTwiceWithChangedRoles_ReflectsLatestState()
    {
        var firstPrincipal  = BuildUser("user-3", "changinguser", "Judge");
        var secondPrincipal = BuildUser("user-3", "changinguser", "Admin");
        var provider        = new TestAuthStateProvider(firstPrincipal);
        var sut             = new CurrentUserState(provider);

        await sut.EnsureInitializedAsync();

        sut.IsJudge.ShouldBeTrue();
        sut.IsAdmin.ShouldBeFalse();

        // simulate role mutation by an admin
        provider.SetPrincipal(secondPrincipal);
        await sut.EnsureInitializedAsync();

        sut.IsAdmin.ShouldBeTrue();
        sut.IsJudge.ShouldBeFalse();
    }

    [Fact]
    public async Task ForceRefreshAsync_IsAliasForEnsureInitializedAsync()
    {
        var principal = BuildUser("user-4", "produceruser", "WineProducer");
        var provider  = new TestAuthStateProvider(principal);
        var sut       = new CurrentUserState(provider);

        await sut.ForceRefreshAsync();

        sut.IsWineProducer.ShouldBeTrue();
        sut.IsJudge.ShouldBeFalse();
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------

    private static ClaimsPrincipal BuildUser(string userId, string name, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, name)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private sealed class TestAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _principal;

        public TestAuthStateProvider(ClaimsPrincipal principal) => _principal = principal;

        public void SetPrincipal(ClaimsPrincipal principal) => _principal = principal;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_principal));
    }
}
