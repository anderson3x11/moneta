using Moneta.Application.Abstractions;

namespace Moneta.Infrastructure.Persistence;

/// <summary>
/// Produces numbers of the form <c>F-YYYY-NNNN</c>, resetting the counter each
/// year while keeping the sequence unbroken within the year.
/// </summary>
public sealed class InvoiceNumberGenerator(IInvoiceRepository invoices) : IInvoiceNumberGenerator
{
    public async Task<string> NextAsync(int year, CancellationToken ct = default)
    {
        var count = await invoices.CountForYearAsync(year, ct);
        return $"F-{year}-{count + 1:D4}";
    }
}
