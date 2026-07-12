using Microsoft.JSInterop;

namespace Moneta.Web.Services;

/// <summary>
/// Holds the signed-in user's JWT for the lifetime of the Blazor circuit, and
/// restores it from browser storage after a reload.
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

    /// <summary>
    /// Restores the session from local storage when the circuit has none yet.
    /// Must be called after the first render, once JS interop is available.
    /// </summary>
    public async Task<bool> RestoreAsync(IJSRuntime js)
    {
        if (IsAuthenticated)
            return true;

        var token = await js.InvokeAsync<string?>("monetaGetToken");
        if (string.IsNullOrEmpty(token))
            return false;

        var email = await js.InvokeAsync<string?>("monetaGetEmail");
        Set(token, email ?? string.Empty);
        return true;
    }
}
