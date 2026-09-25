using WinDbgSymbolsCachingProxy.UpdateCheck;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Uses the running server assembly when the product MSI is not installed.
/// </summary>
public sealed class ServerAssemblyVersionSource : ILocalAssemblyVersionSource
{
    public string? TryGetVersion()
    {
        return ApplicationVersionHelper.TryGetServerVersion();
    }
}
