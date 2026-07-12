using System.Globalization;
using Moneta.Domain.Invoicing;

namespace Moneta.Web.Services;

public static class Format
{
    private static readonly CultureInfo Fr = CultureInfo.GetCultureInfo("fr-FR");

    public static string Euro(decimal amount) => $"{amount.ToString("N2", Fr)} €";

    public static string Date(DateOnly date) => date.ToString("dd/MM/yyyy", Fr);

    public static string StatusLabel(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Draft => "Brouillon",
        InvoiceStatus.Issued => "Émise",
        InvoiceStatus.Paid => "Payée",
        InvoiceStatus.Cancelled => "Annulée",
        _ => status.ToString()
    };

    public static string StatusBadge(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Draft => "text-bg-secondary",
        InvoiceStatus.Issued => "text-bg-primary",
        InvoiceStatus.Paid => "text-bg-success",
        InvoiceStatus.Cancelled => "text-bg-danger",
        _ => "text-bg-secondary"
    };
}
