using Moneta.Domain.Common;

namespace Moneta.Domain.ValueObjects;

/// <summary>
/// A postal address. <see cref="CountryCode"/> is an ISO 3166-1 alpha-2 code,
/// as required by the EN 16931 semantic model.
/// </summary>
public sealed record Address
{
    public string Line1 { get; }
    public string? Line2 { get; }
    public string PostalCode { get; }
    public string City { get; }
    public string CountryCode { get; }

    public Address(string line1, string postalCode, string city, string countryCode = "FR", string? line2 = null)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new DomainException("Address line 1 is required.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Postal code is required.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.");
        if (countryCode is not { Length: 2 })
            throw new DomainException("Country code must be an ISO 3166-1 alpha-2 code.");

        Line1 = line1.Trim();
        Line2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        PostalCode = postalCode.Trim();
        City = city.Trim();
        CountryCode = countryCode.ToUpperInvariant();
    }
}
