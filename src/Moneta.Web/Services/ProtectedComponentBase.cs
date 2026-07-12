using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Moneta.Web.Services;

/// <summary>
/// Base component for pages that require authentication. After the first render
/// (once JS interop is available) it restores the JWT from local storage, and
/// redirects to the login page when no session can be established.
/// </summary>
public abstract class ProtectedComponentBase : ComponentBase
{
    [Inject] protected TokenStore Tokens { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;
    [Inject] protected IJSRuntime JS { get; set; } = default!;

    /// <summary>True once the auth check has completed and the user is signed in.</summary>
    protected bool AuthReady { get; private set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
            return;

        if (!await Tokens.RestoreAsync(JS))
        {
            Nav.NavigateTo("/login");
            return;
        }

        AuthReady = true;
        await OnAuthenticatedAsync();
        StateHasChanged();
    }

    /// <summary>Runs once the session is confirmed. Load page data here.</summary>
    protected virtual Task OnAuthenticatedAsync() => Task.CompletedTask;
}
