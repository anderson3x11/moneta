namespace Moneta.Application.Abstractions;

/// <summary>
/// Allocates the next sequential invoice number. French law requires an
/// unbroken chronological sequence, hence a dedicated generator.
/// </summary>
public interface IInvoiceNumberGenerator
{
    Task<string> NextAsync(int year, CancellationToken ct = default);
}
