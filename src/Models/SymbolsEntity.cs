﻿using MongoDB.Entities;

namespace WinDbgSymbolsCachingProxy.Models;

public class SymbolsEntity : FileEntity
{
    /// <summary>
    ///     The symbol name.
    /// </summary>
    public string Symbol { get; set; } = null!;

    /// <summary>
    ///     The symbol hash.
    /// </summary>
    public string Hash { get; set; } = null!;

    /// <summary>
    ///     The symbol blob file name.
    /// </summary>
    public string File { get; set; } = null!;

    /// <summary>
    ///     Last timestamp that its existence was checked upstream, yet not found.
    /// </summary>
    public DateTime? NotFoundAt { get; set; }

    /// <summary>
    ///     The file name the upstream server originally returned.
    /// </summary>
    public string? UpstreamFileName { get; set; }
    
    /// <summary>
    ///     The last time this entry was requested via REST.
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }

    public override string ToString()
    {
        return $"{Symbol} - {File} ({Hash})";
    }
}