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
}