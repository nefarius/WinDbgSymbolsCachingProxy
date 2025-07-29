using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

public class SymbolsCachedEntity : SymbolsEntity
{
    /// <summary>
    ///     Cached symbol content.
    /// </summary>
    [Ignore]
    public byte[] Blob { get; set; }
}