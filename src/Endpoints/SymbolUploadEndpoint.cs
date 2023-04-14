using FastEndpoints;

using Microsoft.AspNetCore.WebUtilities;

using MongoDB.Entities;

using Smx.PDBSharp;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolUploadEndpoint : EndpointWithoutRequest
{
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
        await foreach (FileMultipartSection? section in FormFileSectionsAsync(ct))
        {
            if (section is null)
            {
                continue;
            }

            using MemoryStream ms = new();
            await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);
            ms.Position = 0;

            using PDBFile? pdb = PDBFile.Open(ms);
            await using DBIReader dbi = pdb.Services.GetService<DBIReader>();

            if (dbi.Header is not DBIHeaderNew hdr)
            {
                await SendErrorsAsync(500, ct);
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
                await SendAsync($"Symbol with name {file} and hash {hash} already exists.", 401, ct);
                continue;
            }

            SymbolsEntity symbol = new()
            {
                Symbol = file,
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

        await SendOkAsync("Upload complete.", ct);
    }
}