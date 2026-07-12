using Moneta.Application.Contracts;
using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Application.Common;

/// <summary>Maps domain entities to their outgoing contracts.</summary>
public static class Mapping
{
    public static AddressDto ToDto(this Address a) =>
        new(a.Line1, a.PostalCode, a.City, a.CountryCode, a.Line2);

    public static Address ToDomain(this AddressDto d) =>
        new(d.Line1, d.PostalCode, d.City, d.CountryCode, d.Line2);

    public static ClientResponse ToResponse(this Client c) =>
        new(c.Id, c.Name, c.Email, c.Address.ToDto(), c.Siret, c.VatNumber, c.CreatedAt);

    public static InvoiceLineResponse ToResponse(this InvoiceLine l) =>
        new(l.Id, l.Description, l.Quantity, l.UnitPriceExclVat, l.VatRate.Percentage, l.NetAmount);

    public static VatBreakdownResponse ToResponse(this VatBreakdown b) =>
        new(b.Rate.Percentage, b.Rate.CategoryCode, b.TaxableBase, b.VatAmount);

    public static InvoiceResponse ToResponse(this Invoice i, string clientName) =>
        new(
            i.Id,
            i.Number,
            i.ClientId,
            clientName,
            i.IssueDate,
            i.DueDate,
            i.Status,
            Invoice.Currency,
            i.Notes,
            i.Lines.Select(l => l.ToResponse()).ToList(),
            i.VatBreakdowns.Select(b => b.ToResponse()).ToList(),
            i.TotalExclVat,
            i.TotalVat,
            i.TotalInclVat);

    public static InvoiceSummaryResponse ToSummary(this Invoice i, string clientName) =>
        new(i.Id, i.Number, clientName, i.IssueDate, i.Status, i.TotalInclVat);
}
