using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Core;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Global")]
public sealed class ServiceConfig
{
    /// <summary>
    ///     The MongoDB database name.
    /// </summary>
    public string DatabaseName { get; set; } = "symbol-cache";

    /// <summary>
    ///     The MongoDB connection string.
    /// </summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017/";

    /// <summary>
    ///     The default upstream a.k.a. "Microsoft Symbol Servers".
    /// </summary>
    public string UpstreamUrl { get; set; } = "https://msdl.microsoft.com/";

    /// <summary>
    ///     Timeout in seconds when the generated badges should be considered expired.
    /// </summary>
    public int BadgeExpiresSeconds { get; set; } = 30;

    /// <summary>
    ///     Period after which 404ed entries should be checked for existence again upstream.
    /// </summary>
    public TimeSpan UpstreamRecheckPeriod { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    ///     Option list of Basic Auth credentials for protected endpoints.
    /// </summary>
    public List<BasicAuthCredentials>? BasicAuthCredentials { get; set; }
}