using Moneta.Domain.Invoicing;

namespace Moneta.Application.Contracts;

public sealed record InvoiceLineRequest(
    string Description,
    decimal Quantity,
    decimal UnitPriceExclVat,
    decimal VatPercentage);

public sealed record CreateInvoiceRequest(
    Guid ClientId,
    IReadOnlyList<InvoiceLineRequest> Lines,
    DateOnly? IssueDate = null,
    DateOnly? DueDate = null,
    string? Notes = null);

public sealed record InvoiceLineResponse(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPriceExclVat,
    decimal VatPercentage,
    decimal NetAmount);

public sealed record VatBreakdownResponse(
    decimal Percentage,
    string CategoryCode,
    decimal TaxableBase,
    decimal VatAmount);

public sealed record InvoiceResponse(
    Guid Id,
    string Number,
    Guid ClientId,
    string ClientName,
    DateOnly IssueDate,
    DateOnly DueDate,
    InvoiceStatus Status,
    string Currency,
    string? Notes,
    IReadOnlyList<InvoiceLineResponse> Lines,
    IReadOnlyList<VatBreakdownResponse> VatBreakdown,
    decimal TotalExclVat,
    decimal TotalVat,
    decimal TotalInclVat);

public sealed record InvoiceSummaryResponse(
    Guid Id,
    string Number,
    string ClientName,
    DateOnly IssueDate,
    InvoiceStatus Status,
    decimal TotalInclVat);
