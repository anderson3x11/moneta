using Moneta.Domain.Invoicing;
using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Tests;

public class InvoiceTotalsTests
{
    private static Invoice NewInvoice() =>
        new("F-2026-0001", Guid.NewGuid(), new DateOnly(2026, 1, 15));

    [Fact]
    public void Single_rate_totals_are_computed()
    {
        var invoice = NewInvoice();
        invoice.AddLine(new InvoiceLine("Consulting", quantity: 2, unitPriceExclVat: 100m, VatRate.Standard20));

        Assert.Equal(200m, invoice.TotalExclVat);
        Assert.Equal(40m, invoice.TotalVat);
        Assert.Equal(240m, invoice.TotalInclVat);
    }

    [Fact]
    public void Multiple_rates_are_grouped_in_the_breakdown()
    {
        var invoice = NewInvoice();
        invoice.AddLine(new InvoiceLine("Service", 1, 100m, VatRate.Standard20));   // VAT 20.00
        invoice.AddLine(new InvoiceLine("Food", 1, 200m, VatRate.Reduced55));       // VAT 11.00

        Assert.Equal(2, invoice.VatBreakdowns.Count);
        Assert.Equal(300m, invoice.TotalExclVat);
        Assert.Equal(31m, invoice.TotalVat);
        Assert.Equal(331m, invoice.TotalInclVat);
    }

    [Fact]
    public void Vat_is_rounded_per_group_half_away_from_zero()
    {
        var invoice = NewInvoice();
        // 33.33 * 20% = 6.666 -> 6.67
        invoice.AddLine(new InvoiceLine("Item", 1, 33.33m, VatRate.Standard20));

        var group = Assert.Single(invoice.VatBreakdowns);
        Assert.Equal(33.33m, group.TaxableBase);
        Assert.Equal(6.67m, group.VatAmount);
        Assert.Equal(40.00m, invoice.TotalInclVat);
    }

    [Fact]
    public void Vat_is_summed_on_the_group_base_not_per_line()
    {
        var invoice = NewInvoice();
        // Two lines at the same rate: base is summed first, then taxed once.
        invoice.AddLine(new InvoiceLine("A", 1, 0.10m, VatRate.Standard20));
        invoice.AddLine(new InvoiceLine("B", 1, 0.10m, VatRate.Standard20));

        var group = Assert.Single(invoice.VatBreakdowns);
        Assert.Equal(0.20m, group.TaxableBase);
        Assert.Equal(0.04m, group.VatAmount);
    }

    [Fact]
    public void Exempt_lines_carry_no_vat()
    {
        var invoice = NewInvoice();
        invoice.AddLine(new InvoiceLine("Exempt service", 1, 500m, VatRate.Exempt));

        Assert.Equal(500m, invoice.TotalExclVat);
        Assert.Equal(0m, invoice.TotalVat);
        Assert.Equal("E", invoice.VatBreakdowns[0].Rate.CategoryCode);
    }
}
