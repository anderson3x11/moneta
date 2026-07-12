using Moneta.Domain.Invoicing;

namespace Moneta.Application.Abstractions;

/// <summary>
/// Produces a Factur-X document: a PDF/A-3 with the EN 16931 CII XML embedded.
/// </summary>
public interface IFacturXGenerator
{
    FacturXDocument Generate(Invoice invoice, Client client, SellerProfile seller);
}

/// <summary>The generated artefacts for one invoice.</summary>
public sealed record FacturXDocument(byte[] Pdf, byte[] Xml, string FileName);
