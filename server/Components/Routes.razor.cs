using Microsoft.AspNetCore.Components;

namespace WinDbgSymbolsCachingProxy.Components;

public partial class Routes
{
    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    /// <summary>
    ///     Full reload of the current URL so the browser can send Basic credentials (same idea as Search/Upload ForceLoad).
    /// </summary>
    private void RetryCurrentRouteWithFullLoad()
    {
        Uri uri = new(Navigation.Uri);
        Navigation.NavigateTo(uri.PathAndQuery, forceLoad: true);
    }
}
