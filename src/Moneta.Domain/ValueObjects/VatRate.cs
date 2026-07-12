using Moneta.Domain.Common;

namespace Moneta.Domain.ValueObjects;

/// <summary>
/// VAT category code from UNTDID 5305, used by the EN 16931 / Factur-X model
/// to qualify a tax rate.
/// </summary>
public enum VatCategory
{
    /// <summary>Standard rate.</summary>
    Standard,

    /// <summary>Zero rated goods.</summary>
    Zero,

    /// <summary>Exempt from tax.</summary>
    Exempt
}

/// <summary>
/// A VAT rate qualified by its category. French statutory rates are exposed as
/// presets; the category drives the code emitted in the structured invoice.
/// </summary>
public sealed record VatRate
{
    public decimal Percentage { get; }
    public VatCategory Category { get; }

    private VatRate(decimal percentage, VatCategory category)
    {
        if (percentage < 0)
            throw new DomainException("A VAT rate cannot be negative.");

        Percentage = percentage;
        Category = category;
    }

    // French statutory rates.
    public static readonly VatRate Standard20 = new(20m, VatCategory.Standard);
    public static readonly VatRate Intermediate10 = new(10m, VatCategory.Standard);
    public static readonly VatRate Reduced55 = new(5.5m, VatCategory.Standard);
    public static readonly VatRate SuperReduced21 = new(2.1m, VatCategory.Standard);
    public static readonly VatRate Exempt = new(0m, VatCategory.Exempt);

    public static VatRate FromPercentage(decimal percentage) => percentage switch
    {
        20m => Standard20,
        10m => Intermediate10,
        5.5m => Reduced55,
        2.1m => SuperReduced21,
        0m => Exempt,
        _ => new VatRate(percentage, VatCategory.Standard)
    };

    /// <summary>UNTDID 5305 code emitted in the CII XML.</summary>
    public string CategoryCode => Category switch
    {
        VatCategory.Standard => "S",
        VatCategory.Zero => "Z",
        VatCategory.Exempt => "E",
        _ => "S"
    };
}
