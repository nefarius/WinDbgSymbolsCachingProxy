using FastEndpoints;

using Microsoft.AspNetCore.WebUtilities;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class SymbolUploadEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<SymbolUploadEndpoint> _logger;
    private readonly SymbolParsingService _parsingService;

    public SymbolUploadEndpoint(ILogger<SymbolUploadEndpoint> logger, SymbolParsingService parsingService)
    {
        _logger = logger;
        _parsingService = parsingService;
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
            using MemoryStream ms = new();
            // keep copy in memory, so we can send it to parser and deliver copy to DB
            await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);
            ms.Position = 0;

            SymbolParsingResult result = await _parsingService.ParseSymbol(filename, ms, ct);

            SymbolsEntity? existingSymbol = null;

            // duplicate check
            if ((await DB.Find<SymbolsEntity>()
                    .ManyAsync(lr =>
                            lr.Eq(r => r.IndexPrefix, result.IndexPrefix) &
                            lr.Eq(r => r.FileName, result.FileName)
                        , ct)).Count != 0)
            {
                if (force.HasValue && force.Value)
                {
                    existingSymbol = (await DB.Find<SymbolsEntity>()
                            .ManyAsync(lr =>
                                    lr.Eq(r => r.IndexPrefix, result.IndexPrefix) &
                                    lr.Eq(r => r.FileName, result.FileName)
                                , ct)
                        ).FirstOrDefault();
                }
                else
                {
                    await SendAsync(
                        $"Symbol with name {filename} and index prefix {result.IndexPrefix} already exists.", 409,
                        ct);
                    return;
                }
            }

            // new or existing entry
            SymbolsEntity symbol = existingSymbol ?? new SymbolsEntity
            {
                IndexPrefix = result.IndexPrefix, FileName = filename
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
}