using FluentValidation;
using Moneta.Application.Contracts;

namespace Moneta.Application.Validation;

public sealed class InvoiceLineRequestValidator : AbstractValidator<InvoiceLineRequest>
{
    private static readonly decimal[] AllowedRates = [20m, 10m, 5.5m, 2.1m, 0m];

    public InvoiceLineRequestValidator()
    {
        RuleFor(l => l.Description).NotEmpty().MaximumLength(500);
        RuleFor(l => l.Quantity).GreaterThan(0);
        RuleFor(l => l.UnitPriceExclVat).GreaterThanOrEqualTo(0);
        RuleFor(l => l.VatPercentage)
            .Must(r => AllowedRates.Contains(r))
            .WithMessage("VAT rate must be one of the French statutory rates: 20, 10, 5.5, 2.1 or 0.");
    }
}

public sealed class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(i => i.ClientId).NotEmpty();
        RuleFor(i => i.Lines).NotEmpty().WithMessage("An invoice needs at least one line.");
        RuleForEach(i => i.Lines).SetValidator(new InvoiceLineRequestValidator());
        RuleFor(i => i.DueDate)
            .GreaterThanOrEqualTo(i => i.IssueDate!.Value)
            .When(i => i is { IssueDate: not null, DueDate: not null })
            .WithMessage("Due date cannot precede the issue date.");
    }
}
