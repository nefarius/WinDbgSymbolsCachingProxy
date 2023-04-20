﻿using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

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
    public string IndexPrefix { get; set; } = null!;

    /// <summary>
    ///     A hex-encoded concatenated string of the Signature and Age where the Signature in "modern" PDB v7 symbols is
    ///     represented in a GUID format and the Age is an unsigned 32-bit integer without leading zeroes.
    /// </summary>
    public string SymbolKey { get; set; } = null!;

    /// <summary>
    ///     The symbol blob file name.
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    ///     The symbol name (left-hand part of the so-called "key" of a symbol).
    /// </summary>
    [Obsolete("Migrate to 'IndexPrefix'")]
    public string Symbol { get; set; } = null!;

    /// <summary>
    ///     The symbol "hash" (misleading name, therefore changed property name but kept DB field name to not break backwards
    ///     compatibility). This is a hex representation of the signature (UInt32 pre-v7 and Guid >=v7) and the Age
    ///     concatenated without leading zeros.
    /// </summary>
    [Obsolete("Migrate to 'SymbolKey' and 'IndexPrefix'")]
    public string Hash { get; set; } = null!;

    /// <summary>
    ///     The symbol blob file name.
    /// </summary>
    [Obsolete("Migrate to 'FileName'")]
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
        return $"{IndexPrefix}/{FileName}";
    }
}