using Moneta.Domain.Common;

namespace Moneta.Domain.Identity;

/// <summary>
/// An application account allowed to sign in and manage invoices. Only the
/// password hash is ever stored.
/// </summary>
public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    private User() { }

    public User(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
    }
}
