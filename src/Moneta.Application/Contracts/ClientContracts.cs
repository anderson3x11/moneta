namespace Moneta.Application.Contracts;

public sealed record AddressDto(
    string Line1,
    string PostalCode,
    string City,
    string CountryCode = "FR",
    string? Line2 = null);

public sealed record CreateClientRequest(
    string Name,
    string Email,
    AddressDto Address,
    string? Siret = null,
    string? VatNumber = null);

public sealed record ClientResponse(
    Guid Id,
    string Name,
    string Email,
    AddressDto Address,
    string? Siret,
    string? VatNumber,
    DateTimeOffset CreatedAt);
