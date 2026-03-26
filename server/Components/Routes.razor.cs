using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinDbgSymbolsCachingProxy.Components;

public partial class Routes
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;
    
    [Inject]
    private IJSRuntime Js { get; set; } = null!;

    /// <summary>
    ///     Forces a request to a protected API route so the browser can re-run Basic Auth challenge, then returns to this page.
    /// </summary>
    private async Task RetryCurrentRouteWithFullLoad()
    {
        Uri uri = new(Navigation.Uri);
        bool authenticated = await Js.InvokeAsync<bool>("symbolsAuthRetry.trigger", uri.PathAndQuery);

        if (authenticated)
        {
            Navigation.NavigateTo(uri.PathAndQuery, forceLoad: true);
        }
    }
}
