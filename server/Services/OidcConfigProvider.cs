using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Services;

/// <summary>
///     Singleton that holds the OIDC configuration loaded from MongoDB at startup.
///     Any change to the configuration requires a service restart to take effect.
/// </summary>
public sealed class OidcConfigProvider
{
    private readonly OidcConfigEntity? _config;

    public OidcConfigProvider(OidcConfigEntity? config)
    {
        _config = config;
    }

    /// <summary>True when an OIDC configuration document exists and <see cref="OidcConfigEntity.Enabled"/> is set.</summary>
    public bool IsOidcEnabled => _config is { Enabled: true };

    /// <summary>The raw configuration document, or null when not configured.</summary>
    public OidcConfigEntity? Config => _config;
}
