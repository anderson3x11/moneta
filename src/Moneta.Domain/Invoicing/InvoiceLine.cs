using Moneta.Domain.Common;
using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Invoicing;

/// <summary>
/// A single billed item. VAT is not carried on the line itself: it is computed
/// per rate group at invoice level, following the EN 16931 calculation rules.
/// </summary>
public class InvoiceLine : Entity
{
    public string Description { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal UnitPriceExclVat { get; private set; }
    public VatRate VatRate { get; private set; } = VatRate.Standard20;

    private InvoiceLine() { }

    public InvoiceLine(string description, decimal quantity, decimal unitPriceExclVat, VatRate vatRate)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Line description is required.");
        if (quantity <= 0)
            throw new DomainException("Line quantity must be strictly positive.");
        if (unitPriceExclVat < 0)
            throw new DomainException("Line unit price cannot be negative.");

        Description = description.Trim();
        Quantity = quantity;
        UnitPriceExclVat = unitPriceExclVat;
        VatRate = vatRate;
    }

    /// <summary>Net line amount excluding VAT (EN 16931 BT-131).</summary>
    public decimal NetAmount => Rounding.Money(Quantity * UnitPriceExclVat);
}
