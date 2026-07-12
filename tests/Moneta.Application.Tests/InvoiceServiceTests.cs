using Moneta.Application.Contracts;
using Moneta.Application.Invoices;
using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Application.Tests;

public class InvoiceServiceTests : IDisposable
{
    private readonly TestHarness _h = new();

    private InvoiceService NewService() =>
        new(_h.Invoices, _h.Clients, _h.Sellers, _h.Numbering, _h.FacturX, _h.UnitOfWork);

    private async Task<Guid> SeedClientAsync()
    {
        var client = new Client("ACME SARL", "billing@acme.fr", new Address("1 rue A", "75001", "Paris"));
        _h.Db.Clients.Add(client);
        await _h.Db.SaveChangesAsync();
        return client.Id;
    }

    private async Task SeedSellerAsync()
    {
        _h.Db.SellerProfiles.Add(new SellerProfile(
            "Studio Moneta SAS", "73282932000074",
            new Address("12 rue de la Facture", "75002", "Paris"),
            "contact@moneta.fr", "FR40732829320"));
        await _h.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Create_allocates_a_sequential_number_and_computes_totals()
    {
        var clientId = await SeedClientAsync();
        var service = NewService();

        var request = new CreateInvoiceRequest(clientId,
        [
            new InvoiceLineRequest("Conseil", 2, 100m, 20m),
            new InvoiceLineRequest("Denrées", 10, 5m, 5.5m)
        ],
        IssueDate: new DateOnly(2026, 5, 1));

        var invoice = await service.CreateAsync(request);

        Assert.Equal("F-2026-0001", invoice.Number);
        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
        Assert.Equal(250m, invoice.TotalExclVat);       // 200 + 50
        Assert.Equal(42.75m, invoice.TotalVat);          // 40 + 2.75
        Assert.Equal(292.75m, invoice.TotalInclVat);
    }

    [Fact]
    public async Task Numbering_increments_within_the_year()
    {
        var clientId = await SeedClientAsync();
        var service = NewService();
        var line = new[] { new InvoiceLineRequest("X", 1, 10m, 20m) };

        var first = await service.CreateAsync(new CreateInvoiceRequest(clientId, line, IssueDate: new DateOnly(2026, 1, 1)));
        var second = await service.CreateAsync(new CreateInvoiceRequest(clientId, line, IssueDate: new DateOnly(2026, 6, 1)));

        Assert.Equal("F-2026-0001", first.Number);
        Assert.Equal("F-2026-0002", second.Number);
    }

    [Fact]
    public async Task Issue_then_pay_moves_the_status()
    {
        var clientId = await SeedClientAsync();
        var service = NewService();
        var created = await service.CreateAsync(new CreateInvoiceRequest(clientId,
            [new InvoiceLineRequest("X", 1, 10m, 20m)]));

        var issued = await service.IssueAsync(created.Id);
        Assert.Equal(InvoiceStatus.Issued, issued.Status);

        var paid = await service.MarkPaidAsync(created.Id);
        Assert.Equal(InvoiceStatus.Paid, paid.Status);
    }

    [Fact]
    public async Task GenerateFacturX_produces_a_pdf_embedding_the_cii_xml()
    {
        var clientId = await SeedClientAsync();
        await SeedSellerAsync();
        var service = NewService();
        var created = await service.CreateAsync(new CreateInvoiceRequest(clientId,
            [new InvoiceLineRequest("Conseil", 1, 100m, 20m)]));
        await service.IssueAsync(created.Id);

        var document = await service.GenerateFacturXAsync(created.Id);

        // PDF magic bytes.
        Assert.Equal("%PDF"u8.ToArray(), document.Pdf[..4]);

        var xml = System.Text.Encoding.UTF8.GetString(document.Xml);
        Assert.Contains("CrossIndustryInvoice", xml);
        Assert.Contains("urn:cen.eu:en16931:2017", xml);
        Assert.Contains("<ram:GrandTotalAmount>120.00</ram:GrandTotalAmount>", xml);
        Assert.EndsWith(".pdf", document.FileName);
    }

    public void Dispose() => _h.Dispose();
}
