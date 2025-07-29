using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     Represents a cached symbol.
/// </summary>
public class SymbolsEntity : FileEntity
{
    /*
     * Relative URI format explanation:
     *
     *
     * |<=============== Index Prefix ===============>|
     *              |<============= Key =============>|
     *                                                |<== File ===>|
     * WerKernel.pdb/B96A69E1F3383F24ED7E10BBD2B3FFC81/WerKernel.pdb
     *               \                             / \
     *                `---------Signature---------´   `Age
     *
     */

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
    ///     The pre-v7 PDB UInt32 Signature.
    /// </summary>
    public ulong? Signature { get; set; }

    /// <summary>
    ///     The "new", current Signature in GUID format.
    /// </summary>
    public Guid? NewSignature { get; set; }

    /// <summary>
    ///     The UInt32 Age value. Increments to distinguish multiple revisions of the same symbol.
    /// </summary>
    public ulong? Age { get; set; }

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
    ///     The last time this entry was requested via REST.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    /// <summary>
    ///     The number of times this symbol got requested, if ever.
    /// </summary>
    public ulong? AccessedCount { get; set; }

    /// <summary>
    ///     Gets whether this symbol has been added via REST API.
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    ///     Gets when this symbol was added manually via file upload.
    /// </summary>
    public DateTime? UploadedAt { get; set; }

    /// <summary>
    ///     Gets the creating timestamp.
    /// </summary>
    public required DateTime? CreatedAt { get; init; }

    /// <summary>
    ///     Gets the relative URI (built from <see cref="IndexPrefix" /> and <see cref="FileName" />).
    /// </summary>
    [Ignore]
    public string RelativeUri => $"{IndexPrefix}{FileName}";

    public override string ToString()
    {
        return RelativeUri;
    }
}