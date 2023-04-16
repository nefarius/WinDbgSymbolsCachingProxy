using FastEndpoints;

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
        // multiple files per request are supported 
        await foreach (FileMultipartSection? section in FormFileSectionsAsync(ct))
        {
            if (section is null)
            {
                continue;
            }

            string filename = section.FileName;
            string extension = Path.GetExtension(section.FileName).ToLowerInvariant();

            using MemoryStream ms = new();
            await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);
            ms.Position = 0;

            string key;
            string signature;
            string name = string.IsNullOrEmpty(section.Name) ? filename : section.Name;

            switch (extension)
            {
                case ".exe":
                case ".dll":
                case ".sys":
                    {
                        key = await ParseExecutable(filename, ms);
                        signature = key.Split('/').Last();
                        break;
                    }
                case ".pdb":
                    {
                        key = await ParsePdb(filename, ms);
                        signature = key.Split('/').Last();
                        break;
                    }
                default:
                    throw new InvalidOperationException($"File {filename} has unsupported extension.");
            }

            // duplicate check
            if ((await DB.Find<SymbolsEntity>()
                    .ManyAsync(lr =>
                            lr.Eq(r => r.Symbol, name) &
                            lr.Eq(r => r.SignatureAge, signature) &
                            lr.Eq(r => r.File, filename)
                        , ct)).Any())
            {
                await SendAsync($"Symbol with name {filename} and key {key} already exists.", 409, ct);
                return;
            }

            // new entry
            SymbolsEntity symbol = new()
            {
                Symbol = name,
                File = filename,
                SignatureAge = signature,
                IsCustom = true,
                UploadedAt = DateTime.UtcNow
            };

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
    /// <returns>The key of the symbol.</returns>
    private Task<string> ParseExecutable(string fileName, MemoryStream stream)
    {
        KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

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
    /// <returns>The key of the PDB.</returns>
    /// <exception cref="FailedToParsePdbException">Thrown on error.</exception>
    private async Task<string> ParsePdb(string fileName, MemoryStream stream)
    {
        using PDBFile? pdb = PDBFile.Open(stream);

        if (pdb is null)
        {
            throw new FailedToParsePdbException($"Couldn't parse {fileName} as PDB file.");
        }

        if (pdb.Type == PDBType.Old)
        {
            throw new FailedToParsePdbException($"The uploaded PDB {fileName} version is not supported.");
        }

        switch (pdb.Type)
        {
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

                    return $"{signature:X}{age:X}".ToUpperInvariant();
                }
            case PDBType.Big:
                {
                    await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

                    if (dbi.Header is not DBIHeaderNew hdr)
                    {
                        throw new FailedToParsePdbException("Failed to parse PDB header.");
                    }

                    uint age = hdr.Age;

                    await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();
                    Guid guid = pdbStream.NewSignature;

                    string key = $"{guid:N}{age:X}".ToUpperInvariant();
                    string indexPrefix = $"{fileName}/{key.ToLowerInvariant()}/";

                    pdb.Dispose();
                    stream.Position = 0;

                    KeyTypeFlags flags = KeyTypeFlags.IdentityKey | KeyTypeFlags.SymbolKey | KeyTypeFlags.ClrKeys;

                    List<SymbolStoreKeyWrapper> keys = _symStore.GetKeys(flags, fileName, stream).ToList();

                    if (!keys.Any())
                    {
                        throw new FailedToParsePdbException("Failed to get symstore keys.");
                    }

                    SymbolStoreKeyWrapper symStoreKey = keys.First();

                    if (!indexPrefix.Equals(symStoreKey.Key.IndexPrefix))
                    {
                        throw new FailedToParsePdbException("PDB parsing signature+age mismatches symstore keys.");
                    }

                    return key;
                }
        }

        throw new FailedToParsePdbException($"Couldn't find the signature of PDB {fileName}.");
    }
}