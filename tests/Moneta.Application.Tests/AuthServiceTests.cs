using Moneta.Application.Auth;
using Moneta.Application.Common;
using Moneta.Application.Contracts;

namespace Moneta.Application.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestHarness _h = new();

    private AuthService NewService() =>
        new(_h.Users, _h.PasswordHasher, _h.Jwt, _h.UnitOfWork);

    [Fact]
    public async Task Register_then_login_issues_a_token()
    {
        var service = NewService();

        var registered = await service.RegisterAsync(new RegisterRequest("user@moneta.fr", "Password123!"));
        Assert.False(string.IsNullOrEmpty(registered.Token));

        var loggedIn = await service.LoginAsync(new LoginRequest("user@moneta.fr", "Password123!"));
        Assert.Equal("user@moneta.fr", loggedIn.Email);
        Assert.False(string.IsNullOrEmpty(loggedIn.Token));
    }

    [Fact]
    public async Task Login_with_wrong_password_is_rejected()
    {
        var service = NewService();
        await service.RegisterAsync(new RegisterRequest("user@moneta.fr", "Password123!"));

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest("user@moneta.fr", "wrong-password")));
    }

    [Fact]
    public async Task Registering_the_same_email_twice_conflicts()
    {
        var service = NewService();
        await service.RegisterAsync(new RegisterRequest("user@moneta.fr", "Password123!"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.RegisterAsync(new RegisterRequest("USER@moneta.fr", "Password123!")));
    }

    [Fact]
    public async Task The_password_is_never_stored_in_clear()
    {
        var service = NewService();
        await service.RegisterAsync(new RegisterRequest("user@moneta.fr", "Password123!"));

        var user = await _h.Users.GetByEmailAsync("user@moneta.fr");
        Assert.NotNull(user);
        Assert.DoesNotContain("Password123!", user!.PasswordHash);
    }

    public void Dispose() => _h.Dispose();
}
