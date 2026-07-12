using Moneta.Application.Abstractions;
using Moneta.Domain.Invoicing;
using QuestPDF.Fluent;

namespace Moneta.Infrastructure.FacturX;

/// <summary>
/// Assembles a Factur-X document: renders the invoice as PDF/A-3, then embeds
/// the EN 16931 CII XML as an associated file named <c>factur-x.xml</c>.
/// </summary>
public sealed class FacturXGenerator : IFacturXGenerator
{
    // The Factur-X specification mandates this exact file name.
    private const string XmlFileName = "factur-x.xml";

    public FacturXDocument Generate(Invoice invoice, Client client, SellerProfile seller)
    {
        var xmlBytes = CiiXmlBuilder.Build(invoice, client, seller);

        var basePdf = new InvoicePdfDocument(invoice, client, seller).GeneratePdf();

        var pdf = EmbedXml(basePdf, xmlBytes);
        var fileName = $"{invoice.Number}.pdf";
        return new FacturXDocument(pdf, xmlBytes, fileName);
    }

    private static byte[] EmbedXml(byte[] pdf, byte[] xmlBytes)
    {
        var work = Path.Combine(Path.GetTempPath(), $"facturx-{Guid.NewGuid():N}");
        Directory.CreateDirectory(work);
        var pdfPath = Path.Combine(work, "invoice.pdf");
        var xmlPath = Path.Combine(work, XmlFileName);
        var outPath = Path.Combine(work, "facturx.pdf");

        try
        {
            File.WriteAllBytes(pdfPath, pdf);
            File.WriteAllBytes(xmlPath, xmlBytes);

            DocumentOperation
                .LoadFile(pdfPath)
                .AddAttachment(new DocumentOperation.DocumentAttachment
                {
                    Key = "factur-x",
                    FilePath = xmlPath,
                    AttachmentName = XmlFileName,
                    MimeType = "text/xml",
                    Description = "Factur-X invoice data (EN 16931)",
                    Relationship = DocumentOperation.DocumentAttachmentRelationship.Alternative
                })
                .Save(outPath);

            return File.ReadAllBytes(outPath);
        }
        finally
        {
            Directory.Delete(work, recursive: true);
        }
    }
}
