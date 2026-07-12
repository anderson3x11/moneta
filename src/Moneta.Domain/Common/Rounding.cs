namespace Moneta.Domain.Common;

public static class Rounding
{
    /// <summary>
    /// Rounds a monetary amount to 2 decimals using commercial rounding
    /// (half away from zero), the convention expected on French invoices.
    /// </summary>
    public static decimal Money(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
