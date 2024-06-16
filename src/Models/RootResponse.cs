using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Models;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public sealed class RootResponse
{
    /// <summary>
    ///     Gets the server assembly version.
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    ///     Gets the project repository URL.
    /// </summary>
    public string ProjectUrl { get; set; } = "https://github.com/nefarius/WinDbgSymbolsCachingProxy";

    /// <summary>
    ///     Gets the total amount of cached symbols
    /// </summary>
    public long CachedSymbolsTotal { get; set; }

    /// <summary>
    ///     Gets the total amount of 404ed cached symbols
    /// </summary>
    public long CachedSymbols404 { get; set; }

    /// <summary>
    ///     Gets the total amount of found cached symbols
    /// </summary>
    public long CachedSymbolsFound { get; set; }
}