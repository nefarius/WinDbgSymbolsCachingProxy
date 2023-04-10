namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolsRequest
{
    public string Symbol { get; set; }

    public string Hash { get; set; }

    public string File { get; set; }
}