using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     Represents a cached symbol.
/// </summary>
public class SymbolsEntityDto
{
    /// <summary>
    ///     The unique identifier of a specific symbol.
    /// </summary>
    public required string IndexPrefix { get; set; }

    /// <summary>
    ///     A hex-encoded concatenated string of the Signature and Age where the Signature in "modern" PDB v7 symbols is
    ///     represented in a GUID format, and the Age is an unsigned 32-bit integer without leading zeroes.
    /// </summary>
    public required string SymbolKey { get; set; }

    /// <summary>
    ///     The symbol-blob file name.
    /// </summary>
    public required string FileName { get; set; }

    /// <summary>
    ///     The last timestamp that its existence was checked upstream, yet not found.
    /// </summary>
    public DateTime? NotFoundAt { get; set; }

    /// <summary>
    ///     The file name that the upstream server originally returned. This will most likely differ since most symbol path
    ///     requests upstream will end in a redirect.
    /// </summary>
    public string? UpstreamFileName { get; set; }

    /// <summary>
    ///     Gets the creating timestamp.
    /// </summary>
    public required DateTime? CreatedAt { get; init; }

    /// <summary>
    ///     Symbol content if <see cref="NotFoundAt"/> is null.  
    /// </summary>
    public byte[] Blob { get; set; }
}