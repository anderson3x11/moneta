using System.Text.RegularExpressions;
using Moneta.Domain.Common;

namespace Moneta.Domain.ValueObjects;

/// <summary>
/// French company establishment identifier (SIRET): 14 digits whose validity
/// is checked with the Luhn algorithm. The first 9 digits are the SIREN.
/// </summary>
public sealed partial class Siret
{
    public string Value { get; }

    private Siret(string value) => Value = value;

    public string Siren => Value[..9];

    public static Siret Create(string? input)
    {
        var digits = (input ?? string.Empty).Replace(" ", string.Empty);

        if (!FourteenDigits().IsMatch(digits))
            throw new DomainException("A SIRET must contain exactly 14 digits.");

        if (!PassesLuhn(digits))
            throw new DomainException($"SIRET '{digits}' fails the Luhn checksum.");

        return new Siret(digits);
    }

    public static bool IsValid(string? input)
    {
        var digits = (input ?? string.Empty).Replace(" ", string.Empty);
        return FourteenDigits().IsMatch(digits) && PassesLuhn(digits);
    }

    private static bool PassesLuhn(string digits)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var d = digits[i] - '0';
            // Double every second digit starting from the right.
            if ((digits.Length - i) % 2 == 0)
            {
                d *= 2;
                if (d > 9) d -= 9;
            }
            sum += d;
        }
        return sum % 10 == 0;
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[0-9]{14}$")]
    private static partial Regex FourteenDigits();
}
