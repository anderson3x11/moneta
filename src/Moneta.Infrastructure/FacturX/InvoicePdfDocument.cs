using System.Globalization;
using Moneta.Domain.Invoicing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Moneta.Infrastructure.FacturX;

/// <summary>The human-readable side of the Factur-X document, rendered as PDF/A-3.</summary>
public sealed class InvoicePdfDocument(Invoice invoice, Client client, SellerProfile seller) : IDocument
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");
    private static readonly Color Accent = Color.FromHex("#2563EB");

    public DocumentMetadata GetMetadata() => new()
    {
        Title = $"Facture {invoice.Number}",
        Author = seller.LegalName,
        Subject = "Facture"
    };

    public DocumentSettings GetSettings() => new()
    {
        PDFA_Conformance = PDFA_Conformance.PDFA_3B
    };

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(40);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(t => t.FontSize(10).FontColor(Colors.Grey.Darken4));

            page.Header().Element(Header);
            page.Content().PaddingVertical(20).Element(Body);
            page.Footer().Element(Footer);
        });
    }

    private void Header(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(seller.LegalName).FontSize(16).SemiBold();
                col.Item().Text(seller.Address.Line1);
                if (!string.IsNullOrWhiteSpace(seller.Address.Line2))
                    col.Item().Text(seller.Address.Line2);
                col.Item().Text($"{seller.Address.PostalCode} {seller.Address.City}");
                col.Item().PaddingTop(4).Text($"SIRET : {seller.Siret}").FontSize(9);
                if (!string.IsNullOrWhiteSpace(seller.VatNumber))
                    col.Item().Text($"TVA : {seller.VatNumber}").FontSize(9);
            });

            row.ConstantItem(200).Column(col =>
            {
                col.Item().AlignRight().Text("FACTURE").FontSize(22).Bold().FontColor(Accent);
                col.Item().AlignRight().Text(invoice.Number).FontSize(12).SemiBold();
                col.Item().PaddingTop(8).AlignRight().Text($"Date : {invoice.IssueDate.ToString("dd/MM/yyyy", Fr)}");
                col.Item().AlignRight().Text($"Échéance : {invoice.DueDate.ToString("dd/MM/yyyy", Fr)}");
            });
        });
    }

    private void Body(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Element(BillTo);
            col.Item().PaddingTop(20).Element(LinesTable);
            col.Item().PaddingTop(15).AlignRight().Element(Totals);

            if (!string.IsNullOrWhiteSpace(invoice.Notes))
                col.Item().PaddingTop(20).Text(invoice.Notes!).Italic().FontColor(Colors.Grey.Darken1);
        });
    }

    private void BillTo(IContainer container)
    {
        container.AlignRight().Width(240).Background(Colors.Grey.Lighten4).Padding(12).Column(col =>
        {
            col.Item().Text("Facturé à").FontSize(9).FontColor(Colors.Grey.Darken2);
            col.Item().Text(client.Name).SemiBold();
            col.Item().Text(client.Address.Line1);
            if (!string.IsNullOrWhiteSpace(client.Address.Line2))
                col.Item().Text(client.Address.Line2);
            col.Item().Text($"{client.Address.PostalCode} {client.Address.City}");
            if (!string.IsNullOrWhiteSpace(client.Siret))
                col.Item().PaddingTop(4).Text($"SIRET : {client.Siret}").FontSize(9);
        });
    }

    private void LinesTable(IContainer container)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(5);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2);
                columns.RelativeColumn(1.5f);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                HeaderCell(header, "Désignation", right: false);
                HeaderCell(header, "Qté", right: true);
                HeaderCell(header, "PU HT", right: true);
                HeaderCell(header, "TVA", right: true);
                HeaderCell(header, "Total HT", right: true);
            });

            foreach (var line in invoice.Lines)
            {
                BodyCell(table).Text(line.Description);
                BodyCell(table).AlignRight().Text(line.Quantity.ToString("0.###", Fr));
                BodyCell(table).AlignRight().Text(Euro(line.UnitPriceExclVat));
                BodyCell(table).AlignRight().Text($"{line.VatRate.Percentage.ToString("0.##", Fr)} %");
                BodyCell(table).AlignRight().Text(Euro(line.NetAmount));
            }
        });
    }

    private void Totals(IContainer container)
    {
        container.Width(260).Column(col =>
        {
            TotalRow(col, "Total HT", invoice.TotalExclVat, false);

            foreach (var breakdown in invoice.VatBreakdowns)
                TotalRow(col, $"TVA {breakdown.Rate.Percentage.ToString("0.##", Fr)} %", breakdown.VatAmount, false);

            col.Item().PaddingTop(4).BorderTop(1).BorderColor(Colors.Grey.Darken1);
            TotalRow(col, "Total TTC", invoice.TotalInclVat, true);
        });
    }

    private void Footer(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(8).Column(col =>
        {
            if (!string.IsNullOrWhiteSpace(seller.Iban))
                col.Item().Text($"Règlement par virement — IBAN : {seller.Iban}").FontSize(8);
            col.Item().Text("En cas de retard de paiement, une indemnité forfaitaire de 40 € pour frais de recouvrement est exigible (art. L441-10 du Code de commerce).")
                .FontSize(7).FontColor(Colors.Grey.Darken1);
        });
    }

    private static void HeaderCell(TableCellDescriptor header, string text, bool right)
    {
        var cell = header.Cell().Background(Accent).Padding(6);
        var aligned = right ? cell.AlignRight() : cell;
        aligned.Text(text).FontColor(Colors.White).SemiBold().FontSize(9);
    }

    private static IContainer BodyCell(TableDescriptor table) =>
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(6).PaddingHorizontal(4);

    private static void TotalRow(ColumnDescriptor col, string label, decimal amount, bool emphasize)
    {
        col.Item().Row(row =>
        {
            var l = row.RelativeItem().Text(label);
            var v = row.ConstantItem(110).AlignRight().Text(Euro(amount));
            if (emphasize)
            {
                l.Bold().FontSize(12);
                v.Bold().FontSize(12);
            }
        });
    }

    private static string Euro(decimal amount) => $"{amount.ToString("N2", Fr)} €";
}
