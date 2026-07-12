using Moneta.Application.Abstractions;
using Moneta.Application.Common;
using Moneta.Application.Contracts;
using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Application.Invoices;

public sealed class InvoiceService(
    IInvoiceRepository invoices,
    IClientRepository clients,
    ISellerProfileRepository sellers,
    IInvoiceNumberGenerator numbering,
    IFacturXGenerator facturX,
    IUnitOfWork uow)
{
    public async Task<IReadOnlyList<InvoiceSummaryResponse>> ListAsync(CancellationToken ct = default)
    {
        var items = await invoices.ListAsync(ct);
        var names = await ClientNamesAsync(items, ct);
        return items.Select(i => i.ToSummary(names[i.ClientId])).ToList();
    }

    public async Task<InvoiceResponse> GetAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await GetInvoiceAsync(id, ct);
        var client = await GetClientAsync(invoice.ClientId, ct);
        return invoice.ToResponse(client.Name);
    }

    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var client = await GetClientAsync(request.ClientId, ct);

        var issueDate = request.IssueDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var number = await numbering.NextAsync(issueDate.Year, ct);

        var invoice = new Invoice(number, client.Id, issueDate, request.DueDate, request.Notes);
        foreach (var line in request.Lines)
        {
            invoice.AddLine(new InvoiceLine(
                line.Description,
                line.Quantity,
                line.UnitPriceExclVat,
                VatRate.FromPercentage(line.VatPercentage)));
        }

        await invoices.AddAsync(invoice, ct);
        await uow.SaveChangesAsync(ct);
        return invoice.ToResponse(client.Name);
    }

    public async Task<InvoiceResponse> IssueAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await GetInvoiceAsync(id, ct);
        invoice.Issue();
        await uow.SaveChangesAsync(ct);
        var client = await GetClientAsync(invoice.ClientId, ct);
        return invoice.ToResponse(client.Name);
    }

    public async Task<InvoiceResponse> MarkPaidAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await GetInvoiceAsync(id, ct);
        invoice.MarkPaid();
        await uow.SaveChangesAsync(ct);
        var client = await GetClientAsync(invoice.ClientId, ct);
        return invoice.ToResponse(client.Name);
    }

    public async Task<InvoiceResponse> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await GetInvoiceAsync(id, ct);
        invoice.Cancel();
        await uow.SaveChangesAsync(ct);
        var client = await GetClientAsync(invoice.ClientId, ct);
        return invoice.ToResponse(client.Name);
    }

    /// <summary>Builds the Factur-X (PDF/A-3 + embedded CII XML) for an invoice.</summary>
    public async Task<FacturXDocument> GenerateFacturXAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await GetInvoiceAsync(id, ct);
        var client = await GetClientAsync(invoice.ClientId, ct);
        var seller = await sellers.GetAsync(ct)
            ?? throw new NotFoundException("No seller profile is configured.");

        return facturX.Generate(invoice, client, seller);
    }

    private async Task<Invoice> GetInvoiceAsync(Guid id, CancellationToken ct) =>
        await invoices.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Invoice {id} was not found.");

    private async Task<Client> GetClientAsync(Guid id, CancellationToken ct) =>
        await clients.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Client {id} was not found.");

    private async Task<Dictionary<Guid, string>> ClientNamesAsync(IEnumerable<Invoice> items, CancellationToken ct)
    {
        var names = new Dictionary<Guid, string>();
        foreach (var clientId in items.Select(i => i.ClientId).Distinct())
        {
            var client = await clients.GetByIdAsync(clientId, ct);
            names[clientId] = client?.Name ?? "(unknown)";
        }
        return names;
    }
}
