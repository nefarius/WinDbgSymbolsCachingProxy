using Microsoft.AspNetCore.Components;

namespace WinDbgSymbolsCachingProxy.Components;

public partial class Routes
{
    private const string AuthChallengePath = "/api/auth/challenge";

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    /// <summary>
    ///     Forces a request to a protected API route so the browser can re-run Basic Auth challenge, then returns to this page.
    /// </summary>
    private void RetryCurrentRouteWithFullLoad()
    {
        Uri uri = new(Navigation.Uri);
        string challengeUrl = $"{AuthChallengePath}?returnUrl={Uri.EscapeDataString(uri.PathAndQuery)}";
        Navigation.NavigateTo(challengeUrl, forceLoad: true);
    }
}
