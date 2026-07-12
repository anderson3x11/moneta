namespace Moneta.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Moneta";
    public string Audience { get; set; } = "Moneta";
    public string SigningKey { get; set; } = string.Empty;
    public int LifetimeMinutes { get; set; } = 60;
}
