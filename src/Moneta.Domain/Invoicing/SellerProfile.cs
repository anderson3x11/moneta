using Moneta.Domain.Common;
using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Invoicing;

/// <summary>
/// The issuer of the invoices (the "seller"). A single profile is held by the
/// application and stamped onto every invoice and structured export.
/// </summary>
public class SellerProfile : Entity
{
    public string LegalName { get; private set; } = string.Empty;
    public string Siret { get; private set; } = string.Empty;
    public string? VatNumber { get; private set; }
    public Address Address { get; private set; } = null!;
    public string? Iban { get; private set; }
    public string ContactEmail { get; private set; } = string.Empty;

    private SellerProfile() { }

    public SellerProfile(string legalName, string siret, Address address, string contactEmail, string? vatNumber = null, string? iban = null)
    {
        if (string.IsNullOrWhiteSpace(legalName))
            throw new DomainException("Seller legal name is required.");

        // Normalises and validates the SIRET through the value object.
        var validated = ValueObjects.Siret.Create(siret);

        LegalName = legalName.Trim();
        Siret = validated.Value;
        Address = address;
        ContactEmail = contactEmail.Trim();
        VatNumber = string.IsNullOrWhiteSpace(vatNumber) ? null : vatNumber.Trim();
        Iban = string.IsNullOrWhiteSpace(iban) ? null : iban.Replace(" ", string.Empty);
    }
}
