using Moneta.Domain.Common;
using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Invoicing;

/// <summary>
/// A customer that invoices are issued to (the "buyer" in EN 16931 terms).
/// </summary>
public class Client : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string? Siret { get; private set; }
    public string? VatNumber { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    // Required by EF Core.
    private Client() { }

    public Client(string name, string email, Address address, string? siret = null, string? vatNumber = null)
    {
        Update(name, email, address, siret, vatNumber);
    }

    public void Update(string name, string email, Address address, string? siret, string? vatNumber)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Client name is required.");
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Client email is required.");
        if (!string.IsNullOrWhiteSpace(siret) && !ValueObjects.Siret.IsValid(siret))
            throw new DomainException($"Invalid SIRET for client '{name}'.");

        Name = name.Trim();
        Email = email.Trim();
        Address = address;
        Siret = string.IsNullOrWhiteSpace(siret) ? null : siret.Replace(" ", string.Empty);
        VatNumber = string.IsNullOrWhiteSpace(vatNumber) ? null : vatNumber.Trim();
    }
}
