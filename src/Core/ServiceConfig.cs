using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Core;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class ServiceConfig
{
    public string DatabaseName { get; set; } = "symbol-cache";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017/";

    public string UpstreamUrl { get; set; } = "https://msdl.microsoft.com/";

    public int BadgeExpiresSeconds { get; set; } = 30;

    public TimeSpan UpstreamRecheckPeriod { get; set; } = TimeSpan.FromDays(7);
}