using System.Collections.Immutable;

using FastEndpoints;

using Microsoft.AspNetCore.WebUtilities;

using MimeDetective;
using MimeDetective.Definitions;
using MimeDetective.Definitions.Licensing;
using MimeDetective.Engine;
using MimeDetective.Storage;

using MongoDB.Entities;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolUploadEndpoint : EndpointWithoutRequest
{
    private static readonly ImmutableArray<Definition> AllDefinitions = new ExhaustiveBuilder
    {
        UsageType = UsageType.PersonalNonCommercial
    }.Build();

    private static readonly ImmutableHashSet<string> Extensions =
        new[] { "pdb", "sys", "exe", "dll" }.ToImmutableHashSet(StringComparer.InvariantCultureIgnoreCase);

    private static readonly ImmutableArray<Definition> ScopedDefinitions = AllDefinitions
        .ScopeExtensions(Extensions)
        .TrimMeta()
        .ToImmutableArray();

    private static readonly ContentInspector Inspector =
        new ContentInspectorBuilder { Definitions = ScopedDefinitions }.Build();

    private readonly ILogger<SymbolUploadEndpoint> _logger;

    public SymbolUploadEndpoint(ILogger<SymbolUploadEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/uploads/symbol");
        AllowFileUploads(true);
        AllowAnonymous();
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

            using MemoryStream ms = new();
            await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);
            ms.Position = 0;

            ImmutableArray<DefinitionMatch> fileTypeDetection = Inspector.Inspect(ms);
            ms.Position = 0;

            if (!fileTypeDetection.Any())
            {
                await SendAsync($"Couldn't detect file type for {section.FileName}", 400, ct);
                return;
            }

            DefinitionMatch detectedType = fileTypeDetection.First();

            // PDB v7 has been uploaded
            if (detectedType.Definition.File is
                { MimeType: "application/x-ms-pdb", Description: "Microsoft Program DataBase (v7)" })
            {
                _logger.LogInformation("{FileName} is a PDB v7, parsing...", section.FileName);
                
                using PDBFile? pdb = PDBFile.Open(ms);
                await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

                if (dbi.Header is not DBIHeaderNew hdr)
                {
                    await SendAsync("Failed to parse PDB header.", 500, ct);
                    return;
                }

                uint age = hdr.Age;

                await using PdbStreamReader? pdbStream = pdb.Services.GetService<PdbStreamReader>();
                Guid guid = pdbStream.NewSignature;

                string hash = $"{guid:N}{age:X}".ToUpperInvariant();
                string file = section.FileName;

                if ((await DB.Find<SymbolsEntity>()
                        .ManyAsync(lr =>
                                lr.Eq(r => r.Hash, hash) &
                                lr.Eq(r => r.File, file)
                            , ct)).Any())
                {
                    await SendAsync($"Symbol with name {file} and hash {hash} already exists.", 409, ct);
                    continue;
                }

                SymbolsEntity symbol = new()
                {
                    Symbol = section.Name,
                    File = file,
                    Hash = hash,
                    IsCustom = true,
                    UploadedAt = DateTime.UtcNow
                };

                ms.Position = 0;

                await symbol.SaveAsync(cancellation: ct);
                await symbol.Data.UploadAsync(ms, cancellation: ct);

                _logger.LogInformation("Added new symbol {Symbol}", symbol);
            }
            
            _logger.LogWarning("Couldn't detect supported file type, skipping {Name}", section.FileName);
            // TODO: implement me
        }

        await SendOkAsync("Upload complete.", ct);
    }
}