using System.Diagnostics.CodeAnalysis;

namespace WinDbgSymbolsCachingProxy.Models;

#nullable disable

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public sealed class SymbolsRequest
{
    public string Symbol { get; set; }

    public string Signature { get; set; }

    public string File { get; set; }
}