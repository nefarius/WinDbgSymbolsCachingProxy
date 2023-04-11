namespace WinDbgSymbolsCachingProxy.Models;

public sealed class RootResponse
{
    public Version? ServerVersion { get; set; }

    public string ProjectUrl { get; set; } = "https://github.com/nefarius/WinDbgSymbolsCachingProxy";

    public ulong? CachedSymbols { get; set; }
}