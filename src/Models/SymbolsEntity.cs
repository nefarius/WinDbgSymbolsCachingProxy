using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

public class SymbolsEntity : FileEntity
{
    // TODO: should we use the symbol "key" as the primary key (ID) in the DB? "Should" be unique after all...

    /// <summary>
    ///     The symbol name (left-hand part of the so-called "key" of a symbol).
    /// </summary>
    public string Symbol { get; set; } = null!;

    /// <summary>
    ///     The symbol "hash" (misleading name, therefore changed property name but kept DB field name to not break backwards
    ///     compatibility). This is a hex representation of the signature (UInt32 pre-v7 and Guid >=v7) and the Age
    ///     concatenated without leading zeros.
    /// </summary>
    [Field("Hash")]
    public string SignatureAge { get; set; } = null!;

    /// <summary>
    ///     The symbol blob file name.
    /// </summary>
    public string File { get; set; } = null!;

    /// <summary>
    ///     Last timestamp that its existence was checked upstream, yet not found.
    /// </summary>
    public DateTime? NotFoundAt { get; set; }

    /// <summary>
    ///     The file name the upstream server originally returned. This will most likely differ since most symbol path requests
    ///     upstream will end in a redirect.
    /// </summary>
    public string? UpstreamFileName { get; set; }

    /// <summary>
    ///     The last time this entry was requested via REST.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    ///     Gets whether this symbol has been added via REST API.
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    ///     Gets when this symbol was added manually via file upload.
    /// </summary>
    public DateTime? UploadedAt { get; set; }

    public override string ToString()
    {
        return $"{Symbol} - {File} ({SignatureAge})";
    }
}