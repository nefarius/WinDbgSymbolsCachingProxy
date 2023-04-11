namespace WinDbgSymbolsCachingProxy.Models;

public sealed class RootResponse
{
    /// <summary>
    ///     Gets the server assembly version.
    /// </summary>
    public Version? ServerVersion { get; set; }

    /// <summary>
    ///     Gets the project repository URL.
    /// </summary>
    public string ProjectUrl { get; set; } = "https://github.com/nefarius/WinDbgSymbolsCachingProxy";

    /// <summary>
    ///     Gets the total amount of cached entries.
    /// </summary>
    public ulong? CachedSymbolsTotal { get; set; }
}