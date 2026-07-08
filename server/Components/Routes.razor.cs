using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinDbgSymbolsCachingProxy.Components;

public partial class Routes
{
    // NavigationManager and OidcConfigProvider are injected via @inject in Routes.razor.

    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    /// <summary>
    ///     Navigates to the OIDC login endpoint with the current page as the return URL.
    /// </summary>
    private void NavigateToLogin()
    {
        string returnUrl = Uri.EscapeDataString(new Uri(Navigation.Uri).PathAndQuery);
        Navigation.NavigateTo($"/account/login?returnUrl={returnUrl}", forceLoad: true);
    }

    /// <summary>
    ///     Forces a request to a protected API route so the browser can re-run the Basic Auth challenge,
    ///     then returns to this page. Only used in Basic-auth mode.
    /// </summary>
    private async Task RetryCurrentRouteWithFullLoad()
    {
        Uri uri = new(Navigation.Uri);

        try
        {
            bool authenticated = await Js.InvokeAsync<bool>("symbolsAuthRetry.trigger");

            if (authenticated)
            {
                Navigation.NavigateTo(uri.PathAndQuery, forceLoad: true);
            }
        }
        catch (JSException)
        {
            // JSInterop failed - do not navigate
        }
    }
}
