using Moneta.Domain.Common;

namespace Moneta.Domain.Invoicing;

/// <summary>
/// Invoice aggregate root. Holds its lines and computes totals following the
/// EN 16931 rules: VAT is summed per rate group, not per line.
/// </summary>
public class Invoice : Entity
{
    public const string Currency = "EUR";

    private readonly List<InvoiceLine> _lines = [];

    public string Number { get; private set; } = string.Empty;
    public Guid ClientId { get; private set; }
    public DateOnly IssueDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public string? Notes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<InvoiceLine> Lines => _lines;

    private Invoice() { }

    public Invoice(string number, Guid clientId, DateOnly issueDate, DateOnly? dueDate = null, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("Invoice number is required.");
        if (clientId == Guid.Empty)
            throw new DomainException("An invoice must reference a client.");

        Number = number.Trim();
        ClientId = clientId;
        IssueDate = issueDate;
        // Default statutory payment term in France is 30 days.
        DueDate = dueDate ?? issueDate.AddDays(30);
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

        if (DueDate < IssueDate)
            throw new DomainException("Due date cannot precede the issue date.");
    }

    public void AddLine(InvoiceLine line)
    {
        EnsureDraft();
        _lines.Add(line);
    }

    public void RemoveLine(Guid lineId)
    {
        EnsureDraft();
        var line = _lines.FirstOrDefault(l => l.Id == lineId)
            ?? throw new DomainException("Line not found on this invoice.");
        _lines.Remove(line);
    }

    public void Issue()
    {
        EnsureDraft();
        if (_lines.Count == 0)
            throw new DomainException("An invoice cannot be issued without lines.");
        Status = InvoiceStatus.Issued;
    }

    public void MarkPaid()
    {
        if (Status != InvoiceStatus.Issued)
            throw new DomainException("Only an issued invoice can be marked as paid.");
        Status = InvoiceStatus.Paid;
    }

    public void Cancel()
    {
        if (Status is InvoiceStatus.Paid)
            throw new DomainException("A paid invoice cannot be cancelled.");
        Status = InvoiceStatus.Cancelled;
    }

    /// <summary>VAT totals grouped by rate (EN 16931 BG-23).</summary>
    public IReadOnlyList<VatBreakdown> VatBreakdowns =>
        _lines
            .GroupBy(l => l.VatRate)
            .OrderByDescending(g => g.Key.Percentage)
            .Select(g =>
            {
                var taxableBase = Rounding.Money(g.Sum(l => l.NetAmount));
                var vatAmount = Rounding.Money(taxableBase * g.Key.Percentage / 100m);
                return new VatBreakdown(g.Key, taxableBase, vatAmount);
            })
            .ToList();

    /// <summary>Total excluding VAT (HT).</summary>
    public decimal TotalExclVat => Rounding.Money(_lines.Sum(l => l.NetAmount));

    /// <summary>Total VAT across all rate groups.</summary>
    public decimal TotalVat => Rounding.Money(VatBreakdowns.Sum(b => b.VatAmount));

    /// <summary>Total including VAT (TTC).</summary>
    public decimal TotalInclVat => Rounding.Money(TotalExclVat + TotalVat);

    private void EnsureDraft()
    {
        if (Status != InvoiceStatus.Draft)
            throw new DomainException("Only a draft invoice can be modified.");
    }
}
