using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Models;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
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