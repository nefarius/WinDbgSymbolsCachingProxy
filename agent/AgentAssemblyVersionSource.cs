using WinDbgSymbolsCachingProxy.UpdateCheck;

namespace HarvestingAgent;

/// <summary>
///     Uses the running agent assembly when the product MSI is not installed.
/// </summary>
public sealed class AgentAssemblyVersionSource : ILocalAssemblyVersionSource
{
    public string? TryGetVersion()
    {
        return ApplicationVersionHelper.TryGetServerVersion();
    }
}
