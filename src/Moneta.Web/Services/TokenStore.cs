namespace Moneta.Web.Services;

/// <summary>
/// Holds the signed-in user's JWT for the lifetime of the Blazor circuit.
/// </summary>
public sealed class TokenStore
{
    public string? Token { get; private set; }
    public string? Email { get; private set; }

    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public void Set(string token, string email)
    {
        Token = token;
        Email = email;
    }

    public void Clear()
    {
        Token = null;
        Email = null;
    }
}
