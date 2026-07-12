using Moneta.Domain.Common;
using Moneta.Domain.ValueObjects;

namespace Moneta.Domain.Tests;

public class SiretTests
{
    [Theory]
    [InlineData("73282932000074")]
    [InlineData("732 829 320 00074")] // spaces are tolerated
    public void Create_accepts_a_valid_siret(string input)
    {
        var siret = Siret.Create(input);

        Assert.Equal("73282932000074", siret.Value);
        Assert.Equal("732829320", siret.Siren);
    }

    [Theory]
    [InlineData("73282932000075")] // last digit breaks the checksum
    [InlineData("12345678901234")]
    public void Create_rejects_a_bad_checksum(string input)
    {
        Assert.Throws<DomainException>(() => Siret.Create(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("1234567890")]      // too short
    [InlineData("732829320000749")] // too long
    [InlineData("7328293200007A")]  // non-digit
    public void Create_rejects_wrong_shapes(string input)
    {
        Assert.Throws<DomainException>(() => Siret.Create(input));
    }

    [Fact]
    public void IsValid_matches_Create()
    {
        Assert.True(Siret.IsValid("73282932000074"));
        Assert.False(Siret.IsValid("73282932000075"));
        Assert.False(Siret.IsValid(null));
    }
}
