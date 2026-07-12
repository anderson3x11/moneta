using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Invoicing;

/// <summary>
/// VAT total for one rate group (EN 16931 BG-23). One entry per distinct rate
/// appearing on the invoice.
/// </summary>
public sealed record VatBreakdown(
    VatRate Rate,
    decimal TaxableBase,
    decimal VatAmount);
