using Moneta.Domain.Identity;

namespace Moneta.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IJwtTokenService
{
    /// <summary>Issues a signed JWT and returns the token with its UTC expiry.</summary>
    (string Token, DateTimeOffset ExpiresAt) Issue(User user);
}
