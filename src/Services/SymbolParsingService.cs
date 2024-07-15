﻿using System.Diagnostics.CodeAnalysis;

using Microsoft.SymbolStore.KeyGenerators;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Core.Exceptions;

namespace WinDbgSymbolsCachingProxy.Services;

public sealed record SymbolParsingResult(
    string FileName,
    string IndexPrefix,
    uint? Age = default,
    ulong? Signature = default,
    Guid? NewSignature = default);

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class SymbolParsingService
{
    private readonly ILogger<SymbolParsingService> _logger;
    private readonly SymStoreService _symStore;

    public SymbolParsingService(ILogger<SymbolParsingService> logger, SymStoreService symStore)
    {
        _logger = logger;
        _symStore = symStore;
    }

    /// <summary>
    ///     Extracts a <see cref="SymbolParsingResult" /> from a given symbol file.
    /// </summary>
    /// <param name="filename">The file name of the symbol.</param>
    /// <param name="stream">A <see cref="Stream" /> containing the binary content.</param>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns>A <see cref="SymbolParsingResult" />.</returns>
    /// <exception cref="UnsupportedFileTypeException">An unsupported file type/extension was provided.</exception>
    /// <exception cref="FailedToParseExecutableException">Failed to parse the provided executable (.exe, .dll, ...).</exception>
    /// <exception cref="FailedToParsePdbException">Failed to parse the provided debug symbols file (.pdb).</exception>
    public async Task<SymbolParsingResult> ParseSymbol(string filename, Stream stream, CancellationToken ct = default)
    {
        using MemoryStream ms = new();
        await stream.CopyToAsync(ms, 1024 * 64, ct);
        ms.Position = 0;

        return await ParseSymbol(filename, ms, ct);
    }

    /// <summary>
    ///     Extracts a <see cref="SymbolParsingResult" /> from a given symbol file.
    /// </summary>
    /// <param name="filename">The file name of the symbol.</param>
    /// <param name="stream">A <see cref="MemoryStream" /> containing the binary content.</param>
    /// <param name="ct">Optional <see cref="CancellationToken" />.</param>
    /// <returns>A <see cref="SymbolParsingResult" />.</returns>
    /// <exception cref="UnsupportedFileTypeException">An unsupported file type/extension was provided.</exception>
    /// <exception cref="FailedToParseExecutableException">Failed to parse the provided executable (.exe, .dll, ...).</exception>
    /// <exception cref="FailedToParsePdbException">Failed to parse the provided debug symbols file (.pdb).</exception>
    public async Task<SymbolParsingResult> ParseSymbol(string filename, MemoryStream stream,
        CancellationToken ct = default)
    {
        filename = filename.ToLowerInvariant();
        string extension = Path.GetExtension(filename).ToLowerInvariant();

        string indexPrefix;
        string signature;

        switch (extension)
        {
            case ".exe":
            case ".dll":
            case ".sys":
                {
                    indexPrefix = await ParseExecutable(filename, stream);
                    signature = indexPrefix.Split('/')[1].ToUpper();
                    _logger.LogInformation("File {File} (EXE/DLL/SYS) has signature {Signature}", filename,
                        signature);
                    return new SymbolParsingResult(filename, indexPrefix);
                }
            case ".pdb":
                {
                    PdbParsingResult result = await ParsePdb(filename, stream);
                    indexPrefix = result.IndexPrefix;
                    signature = indexPrefix.Split('/')[1].ToUpper();
                    _logger.LogInformation("File {File} (PDB) has signature {Signature}", filename, signature);
                    return new SymbolParsingResult(filename, indexPrefix, result.Age, result.Signature,
                        result.NewSignature);
                }
            default:
                throw new UnsupportedFileTypeException($"File {filename} has unsupported extension.");
        }
    }

    /// <summary>
    ///     Parses various portable executable formats.
    /// </summary>
    /// <param name="fileName">The file  name (with extension, without path).</param>
    /// <param name="stream">The stream containing the file content.</param>
    /// <returns>The index prefix of the symbol.</returns>
    private Task<string> ParseExecutable(string fileName, MemoryStream stream)
    {
        const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

        IEnumerable<SymbolStoreKeyWrapper> keys = _symStore.GetKeys(flags, fileName, stream).ToList();

        if (!keys.Any())
        {
            throw new FailedToParseExecutableException($"Couldn't get any keys for file {fileName}.");
        }

        SymbolStoreKeyWrapper key = keys.First();

        return Task.FromResult(key.Key.IndexPrefix.ToLowerInvariant());
    }

    /// <summary>
    ///     Parses various versions of PDB files.
    /// </summary>
    /// <param name="fileName">The file  name (with extension, without path).</param>
    /// <param name="stream">The stream containing the file content.</param>
    /// <returns>The index prefix of the PDB.</returns>
    /// <exception cref="FailedToParsePdbException">Thrown on error.</exception>
    private async Task<PdbParsingResult> ParsePdb(string fileName, MemoryStream stream)
    {
        using PDBFile? pdb = PDBFile.Open(stream);

        if (pdb is null)
        {
            throw new FailedToParsePdbException($"Couldn't parse {fileName} as PDB file.");
        }

        switch (pdb.Type)
        {
            case PDBType.Old:
                throw new FailedToParsePdbException($"The uploaded PDB {fileName} version is not supported.");
            case PDBType.Small:
                {
                    await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        throw new FailedToParsePdbException("Failed to parse PDB header.");
                    }

                    PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                    if (pdbStream is null)
                    {
                        throw new FailedToParsePdbException("Failed to get PDB stream.");
                    }

                    uint age = hdr.Age;
                    uint signature = pdbStream.Signature;

                    string key = $"{signature:X}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key}/".ToLowerInvariant();

                    return new PdbParsingResult(indexPrefix, age, signature);
                }
            case PDBType.Big:
                {
                    List<SymbolStoreKeyWrapper> keys;
                    const KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

                    await using DBIReader? dbi = pdb.Services.GetService<DBIReader?>();

                    if (dbi is null)
                    {
                        _logger.LogWarning("Couldn't get DBIReader, using symstore fallback");

                        keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                        return new PdbParsingResult(keys.First().Key.IndexPrefix.ToLowerInvariant());
                    }

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        _logger.LogWarning("Couldn't get DBIHeaderNew, using symstore fallback");

                        keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                        return new PdbParsingResult(keys.First().Key.IndexPrefix.ToLowerInvariant());
                    }

                    uint age = hdr.Age;

                    await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                    if (pdbStream.NewSignature is null)
                    {
                        _logger.LogWarning("Couldn't get NewSignature, using symstore fallback");

                        keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                        return new PdbParsingResult(keys.First().Key.IndexPrefix.ToLowerInvariant());
                    }

                    Guid guid = pdbStream.NewSignature.Value;

                    string key = $"{guid:N}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key}/".ToLowerInvariant();

                    // ReSharper disable once DisposeOnUsingVariable
                    pdb.Dispose();
                    stream.Position = 0;

                    keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                    if (keys.Count == 0)
                    {
                        throw new FailedToParsePdbException("Failed to get symstore keys.");
                    }

                    SymbolStoreKeyWrapper symStoreKey = keys.First();

                    if (!indexPrefix.Equals(symStoreKey.Key.IndexPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FailedToParsePdbException("PDB parsing signature+age mismatches symstore keys.");
                    }

                    return new PdbParsingResult(indexPrefix, age, NewSignature: guid);
                }
            default:
                throw new FailedToParsePdbException($"Couldn't find the signature of PDB {fileName}.");
        }
    }

    private record PdbParsingResult(
        string IndexPrefix,
        uint? Age = default,
        uint? Signature = default,
        Guid? NewSignature = default);
}