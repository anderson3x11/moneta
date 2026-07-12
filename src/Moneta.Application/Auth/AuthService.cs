using Moneta.Application.Abstractions;
using Moneta.Application.Common;
using Moneta.Application.Contracts;
using Moneta.Domain.Identity;

namespace Moneta.Application.Auth;

public sealed class AuthService(
    IUserRepository users,
    IPasswordHasher hasher,
    IJwtTokenService tokens,
    IUnitOfWork uow)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await users.ExistsAsync(email, ct))
            throw new ConflictException("An account with this email already exists.");

        var user = new User(email, hasher.Hash(request.Password));
        await users.AddAsync(user, ct);
        await uow.SaveChangesAsync(ct);

        return IssueFor(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await users.GetByEmailAsync(email, ct);

        // Same error whether the account is missing or the password is wrong.
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid email or password.");

        return IssueFor(user);
    }

    private AuthResponse IssueFor(User user)
    {
        var (token, expiresAt) = tokens.Issue(user);
        return new AuthResponse(token, expiresAt, user.Email);
    }
}
