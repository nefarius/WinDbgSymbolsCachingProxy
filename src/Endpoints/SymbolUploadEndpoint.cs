﻿using FastEndpoints;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.SymbolStore.KeyGenerators;

using MongoDB.Entities;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Core.Exceptions;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolUploadEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<SymbolUploadEndpoint> _logger;
    private readonly SymStoreService _symStore;

    public SymbolUploadEndpoint(ILogger<SymbolUploadEndpoint> logger, SymStoreService symStore)
    {
        _logger = logger;
        _symStore = symStore;
    }

    public override void Configure()
    {
        Post("/api/uploads/symbol");
        AllowFileUploads(true);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        bool? force = Query<bool?>("force", false);

        // multiple files per request are supported 
        await foreach (FileMultipartSection? section in FormFileSectionsAsync(ct))
        {
            if (section is null)
            {
                continue;
            }

            string filename = section.FileName.ToLowerInvariant();
            string extension = Path.GetExtension(filename).ToLowerInvariant();

            using MemoryStream ms = new();
            await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);
            ms.Position = 0;

            string indexPrefix;
            string signature;

            switch (extension)
            {
                case ".exe":
                case ".dll":
                case ".sys":
                    {
                        indexPrefix = (await ParseExecutable(filename, ms)).ToLowerInvariant();
                        signature = indexPrefix.Split('/')[1].ToUpper();
                        _logger.LogInformation("File {File} (EXE/DLL/SYS) has signature {Signature}", filename,
                            signature);
                        break;
                    }
                case ".pdb":
                    {
                        indexPrefix = (await ParsePdb(filename, ms)).ToLowerInvariant();
                        signature = indexPrefix.Split('/')[1].ToUpper();
                        _logger.LogInformation("File {File} (PDB) has signature {Signature}", filename, signature);
                        break;
                    }
                default:
                    throw new InvalidOperationException($"File {filename} has unsupported extension.");
            }

            _logger.LogInformation("Got key {IndexPrefix} with signature {Signature}", indexPrefix, signature);

            SymbolsEntity? existingSymbol = null;

            // duplicate check
            if ((await DB.Find<SymbolsEntity>()
                    .ManyAsync(lr =>
                            lr.Eq(r => r.IndexPrefix, indexPrefix) &
                            lr.Eq(r => r.FileName, filename)
                        , ct)).Any())
            {
                if (force.HasValue && force.Value)
                {
                    existingSymbol = (await DB.Find<SymbolsEntity>()
                            .ManyAsync(lr =>
                                    lr.Eq(r => r.IndexPrefix, indexPrefix) &
                                    lr.Eq(r => r.FileName, filename)
                                , ct)
                        ).FirstOrDefault();
                }
                else
                {
                    await SendAsync($"Symbol with name {filename} and index prefix {indexPrefix} already exists.", 409,
                        ct);
                    return;
                }
            }

            // new or existing entry
            SymbolsEntity symbol = existingSymbol ?? new SymbolsEntity
            {
                IndexPrefix = indexPrefix, FileName = filename
            };

            symbol.IsCustom = true;
            symbol.UploadedAt = DateTime.UtcNow;
            symbol.NotFoundAt = null;

            ms.Position = 0;

            // upload blob
            await symbol.SaveAsync(cancellation: ct);
            await symbol.Data.UploadAsync(ms, cancellation: ct);

            _logger.LogInformation("Added new symbol {Symbol}", symbol);
        }

        await SendOkAsync("Upload complete.", ct);
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

        return Task.FromResult(key.Key.IndexPrefix);
    }

    /// <summary>
    ///     Parses various versions of PDB files.
    /// </summary>
    /// <param name="fileName">The file  name (with extension, without path).</param>
    /// <param name="stream">The stream containing the file content.</param>
    /// <returns>The index prefix of the PDB.</returns>
    /// <exception cref="FailedToParsePdbException">Thrown on error.</exception>
    private async Task<string> ParsePdb(string fileName, MemoryStream stream)
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
                    return $"{fileName}/{key}/";
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

                        return keys.First().Key.IndexPrefix;
                    }

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        _logger.LogWarning("Couldn't get DBIHeaderNew, using symstore fallback");

                        keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                        return keys.First().Key.IndexPrefix;
                    }

                    uint age = hdr.Age;

                    await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();

                    if (pdbStream.NewSignature is null)
                    {
                        _logger.LogWarning("Couldn't get NewSignature, using symstore fallback");

                        keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                        return keys.First().Key.IndexPrefix;
                    }

                    Guid guid = pdbStream.NewSignature.Value;

                    string key = $"{guid:N}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key}/";

                    pdb.Dispose();
                    stream.Position = 0;

                    keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                    if (!keys.Any())
                    {
                        throw new FailedToParsePdbException("Failed to get symstore keys.");
                    }

                    SymbolStoreKeyWrapper symStoreKey = keys.First();

                    if (!indexPrefix.Equals(symStoreKey.Key.IndexPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new FailedToParsePdbException("PDB parsing signature+age mismatches symstore keys.");
                    }

                    return indexPrefix;
                }
            default:
                throw new FailedToParsePdbException($"Couldn't find the signature of PDB {fileName}.");
        }
    }
}