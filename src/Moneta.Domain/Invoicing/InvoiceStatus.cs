namespace Moneta.Domain.Invoicing;

public enum InvoiceStatus
{
    /// <summary>Editable, not yet issued to the client.</summary>
    Draft,

    /// <summary>Finalised and legally issued; lines are frozen.</summary>
    Issued,

    /// <summary>Payment received.</summary>
    Paid,

    /// <summary>Cancelled after issuance.</summary>
    Cancelled
}
