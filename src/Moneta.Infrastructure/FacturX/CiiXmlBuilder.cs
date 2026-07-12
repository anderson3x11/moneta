using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Moneta.Domain.Invoicing;

namespace Moneta.Infrastructure.FacturX;

/// <summary>
/// Builds the Cross Industry Invoice (CII) XML that Factur-X embeds, following
/// the EN 16931 semantic model. Amounts are rendered with the invariant culture.
/// </summary>
public static class CiiXmlBuilder
{
    private static readonly XNamespace Rsm = "urn:un:unece:uncefact:data:standard:CrossIndustryInvoice:100";
    private static readonly XNamespace Ram = "urn:un:unece:uncefact:data:standard:ReusableAggregateBusinessInformationEntity:100";
    private static readonly XNamespace Udt = "urn:un:unece:uncefact:data:standard:UnqualifiedDataType:100";

    // Factur-X EN 16931 (Comfort) guideline identifier.
    private const string GuidelineId = "urn:cen.eu:en16931:2017#compliant#urn:factur-x.eu:1p0:en16931";
    private const string CommercialInvoice = "380";
    private const string DateFormat = "102"; // CCYYMMDD

    public static byte[] Build(Invoice invoice, Client client, SellerProfile seller)
    {
        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(Rsm + "CrossIndustryInvoice",
                new XAttribute(XNamespace.Xmlns + "rsm", Rsm),
                new XAttribute(XNamespace.Xmlns + "ram", Ram),
                new XAttribute(XNamespace.Xmlns + "udt", Udt),
                Context(),
                Document(invoice),
                Transaction(invoice, client, seller)));

        var settings = new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            Indent = true
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
            doc.Save(writer);

        return stream.ToArray();
    }

    private static XElement Context() =>
        new(Rsm + "ExchangedDocumentContext",
            new XElement(Ram + "GuidelineSpecifiedDocumentContextParameter",
                new XElement(Ram + "ID", GuidelineId)));

    private static XElement Document(Invoice invoice) =>
        new(Rsm + "ExchangedDocument",
            new XElement(Ram + "ID", invoice.Number),
            new XElement(Ram + "TypeCode", CommercialInvoice),
            new XElement(Ram + "IssueDateTime",
                new XElement(Udt + "DateTimeString",
                    new XAttribute("format", DateFormat),
                    Date(invoice.IssueDate))));

    private static XElement Transaction(Invoice invoice, Client client, SellerProfile seller)
    {
        var transaction = new XElement(Rsm + "SupplyChainTradeTransaction");

        var lineId = 1;
        foreach (var line in invoice.Lines)
            transaction.Add(LineItem(lineId++, line));

        transaction.Add(HeaderAgreement(seller, client));
        transaction.Add(new XElement(Ram + "ApplicableHeaderTradeDelivery"));
        transaction.Add(HeaderSettlement(invoice));
        return transaction;
    }

    private static XElement LineItem(int lineId, InvoiceLine line) =>
        new(Ram + "IncludedSupplyChainTradeLineItem",
            new XElement(Ram + "AssociatedDocumentLineDocument",
                new XElement(Ram + "LineID", lineId)),
            new XElement(Ram + "SpecifiedTradeProduct",
                new XElement(Ram + "Name", line.Description)),
            new XElement(Ram + "SpecifiedLineTradeAgreement",
                new XElement(Ram + "NetPriceProductTradePrice",
                    new XElement(Ram + "ChargeAmount", Amount(line.UnitPriceExclVat)))),
            new XElement(Ram + "SpecifiedLineTradeDelivery",
                new XElement(Ram + "BilledQuantity",
                    new XAttribute("unitCode", "C62"),
                    Number(line.Quantity))),
            new XElement(Ram + "SpecifiedLineTradeSettlement",
                new XElement(Ram + "ApplicableTradeTax",
                    new XElement(Ram + "TypeCode", "VAT"),
                    new XElement(Ram + "CategoryCode", line.VatRate.CategoryCode),
                    new XElement(Ram + "RateApplicablePercent", Percent(line.VatRate.Percentage))),
                new XElement(Ram + "SpecifiedTradeSettlementLineMonetarySummation",
                    new XElement(Ram + "LineTotalAmount", Amount(line.NetAmount)))));

    private static XElement HeaderAgreement(SellerProfile seller, Client client) =>
        new(Ram + "ApplicableHeaderTradeAgreement",
            SellerParty(seller),
            BuyerParty(client));

    private static XElement SellerParty(SellerProfile seller)
    {
        var party = new XElement(Ram + "SellerTradeParty",
            new XElement(Ram + "Name", seller.LegalName),
            new XElement(Ram + "SpecifiedLegalOrganization",
                new XElement(Ram + "ID", new XAttribute("schemeID", "0002"), seller.Siret)),
            PostalAddress(seller.Address));

        if (!string.IsNullOrWhiteSpace(seller.VatNumber))
            party.Add(TaxRegistration(seller.VatNumber));

        return party;
    }

    private static XElement BuyerParty(Client client)
    {
        var party = new XElement(Ram + "BuyerTradeParty",
            new XElement(Ram + "Name", client.Name));

        if (!string.IsNullOrWhiteSpace(client.Siret))
            party.Add(new XElement(Ram + "SpecifiedLegalOrganization",
                new XElement(Ram + "ID", new XAttribute("schemeID", "0002"), client.Siret)));

        party.Add(PostalAddress(client.Address));

        if (!string.IsNullOrWhiteSpace(client.VatNumber))
            party.Add(TaxRegistration(client.VatNumber));

        return party;
    }

    private static XElement PostalAddress(Domain.ValueObjects.Address address)
    {
        var element = new XElement(Ram + "PostalTradeAddress",
            new XElement(Ram + "PostcodeCode", address.PostalCode),
            new XElement(Ram + "LineOne", address.Line1));

        if (!string.IsNullOrWhiteSpace(address.Line2))
            element.Add(new XElement(Ram + "LineTwo", address.Line2));

        element.Add(new XElement(Ram + "CityName", address.City));
        element.Add(new XElement(Ram + "CountryID", address.CountryCode));
        return element;
    }

    private static XElement TaxRegistration(string vatNumber) =>
        new(Ram + "SpecifiedTaxRegistration",
            new XElement(Ram + "ID", new XAttribute("schemeID", "VA"), vatNumber));

    private static XElement HeaderSettlement(Invoice invoice)
    {
        var settlement = new XElement(Ram + "ApplicableHeaderTradeSettlement",
            new XElement(Ram + "InvoiceCurrencyCode", Invoice.Currency));

        foreach (var breakdown in invoice.VatBreakdowns)
            settlement.Add(new XElement(Ram + "ApplicableTradeTax",
                new XElement(Ram + "CalculatedAmount", Amount(breakdown.VatAmount)),
                new XElement(Ram + "TypeCode", "VAT"),
                new XElement(Ram + "BasisAmount", Amount(breakdown.TaxableBase)),
                new XElement(Ram + "CategoryCode", breakdown.Rate.CategoryCode),
                new XElement(Ram + "RateApplicablePercent", Percent(breakdown.Rate.Percentage))));

        settlement.Add(new XElement(Ram + "SpecifiedTradePaymentTerms",
            new XElement(Ram + "DueDateDateTime",
                new XElement(Udt + "DateTimeString",
                    new XAttribute("format", DateFormat),
                    Date(invoice.DueDate)))));

        settlement.Add(new XElement(Ram + "SpecifiedTradeSettlementHeaderMonetarySummation",
            new XElement(Ram + "LineTotalAmount", Amount(invoice.TotalExclVat)),
            new XElement(Ram + "TaxBasisTotalAmount", Amount(invoice.TotalExclVat)),
            new XElement(Ram + "TaxTotalAmount",
                new XAttribute("currencyID", Invoice.Currency),
                Amount(invoice.TotalVat)),
            new XElement(Ram + "GrandTotalAmount", Amount(invoice.TotalInclVat)),
            new XElement(Ram + "DuePayableAmount", Amount(invoice.TotalInclVat))));

        return settlement;
    }

    private static string Amount(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
    private static string Number(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    private static string Percent(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);
    private static string Date(DateOnly value) => value.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
}
