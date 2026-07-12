using FluentValidation;
using Moneta.Application.Contracts;
using Moneta.Domain.ValueObjects;

namespace Moneta.Application.Validation;

public sealed class AddressDtoValidator : AbstractValidator<AddressDto>
{
    public AddressDtoValidator()
    {
        RuleFor(a => a.Line1).NotEmpty();
        RuleFor(a => a.PostalCode).NotEmpty();
        RuleFor(a => a.City).NotEmpty();
        RuleFor(a => a.CountryCode).NotEmpty().Length(2);
    }
}

public sealed class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
        RuleFor(c => c.Address).NotNull().SetValidator(new AddressDtoValidator());
        RuleFor(c => c.Siret)
            .Must(Siret.IsValid)
            .When(c => !string.IsNullOrWhiteSpace(c.Siret))
            .WithMessage("The SIRET is not valid.");
    }
}
