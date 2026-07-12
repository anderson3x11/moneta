using Moneta.Domain.Common;
using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Tests;

public class InvoiceLifecycleTests
{
    private static Invoice DraftWithOneLine()
    {
        var invoice = new Invoice("F-2026-0002", Guid.NewGuid(), new DateOnly(2026, 2, 1));
        invoice.AddLine(new InvoiceLine("Service", 1, 100m, VatRate.Standard20));
        return invoice;
    }

    [Fact]
    public void Due_date_defaults_to_thirty_days()
    {
        var invoice = new Invoice("F-2026-0003", Guid.NewGuid(), new DateOnly(2026, 3, 1));
        Assert.Equal(new DateOnly(2026, 3, 31), invoice.DueDate);
    }

    [Fact]
    public void Issuing_freezes_the_lines()
    {
        var invoice = DraftWithOneLine();
        invoice.Issue();

        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Throws<DomainException>(() =>
            invoice.AddLine(new InvoiceLine("Late", 1, 10m, VatRate.Standard20)));
    }

    [Fact]
    public void Cannot_issue_an_empty_invoice()
    {
        var invoice = new Invoice("F-2026-0004", Guid.NewGuid(), new DateOnly(2026, 4, 1));
        Assert.Throws<DomainException>(invoice.Issue);
    }

    [Fact]
    public void Payment_requires_an_issued_invoice()
    {
        var invoice = DraftWithOneLine();
        Assert.Throws<DomainException>(invoice.MarkPaid);

        invoice.Issue();
        invoice.MarkPaid();
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public void A_paid_invoice_cannot_be_cancelled()
    {
        var invoice = DraftWithOneLine();
        invoice.Issue();
        invoice.MarkPaid();

        Assert.Throws<DomainException>(invoice.Cancel);
    }
}
