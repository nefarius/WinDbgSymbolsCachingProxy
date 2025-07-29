using FastEndpoints;

using Microsoft.AspNetCore.WebUtilities;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Allows uploading custom symbol files to the database.
/// </summary>
internal sealed class SymbolUploadEndpoint(ILogger<SymbolUploadEndpoint> logger, SymbolParsingService parsingService)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Post("/api/uploads/symbol");
        AllowFileUploads(true);
        Options(x => x.WithTags("Symbols"));
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

            try
            {
                string filename = section.FileName.ToLowerInvariant();
                using MemoryStream ms = new();
                // keep copy in memory, so we can send it to parser and deliver copy to DB
                await section.Section.Body.CopyToAsync(ms, 1024 * 64, ct);
                ms.Position = 0;

                SymbolParsingResult result;

                try
                {
                    result = await parsingService.ParseSymbol(filename, ms, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to parse {File}", filename);
                    AddError($"Failed to parse file {filename}, error: {ex}", "500");
                    continue;
                }

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
                        logger.LogError("Symbol with name {Filename} and index prefix {IndexPrefix} already exists",
                            filename, result.IndexPrefix);
                        await Send.ResponseAsync(
                            $"Symbol with name {filename} and index prefix {result.IndexPrefix} already exists.", 409,
                            ct);
                        return;
                    }
                }

                // new or existing entry
                SymbolsEntity symbol = existingSymbol ?? new SymbolsEntity
                {
                    CreatedAt = DateTime.UtcNow,
                    IndexPrefix = result.IndexPrefix,
                    SymbolKey = result.SymbolKey,
                    FileName = result.FileName
                };

                symbol.IsCustom = true;
                symbol.UploadedAt = DateTime.UtcNow;
                symbol.NotFoundAt = null;

                ms.Position = 0;

                // upload blob
                await symbol.SaveAsync(cancellation: ct);
                await symbol.Data.UploadAsync(ms, cancellation: ct);

                logger.LogInformation("Added new symbol {Symbol}", symbol);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to upload symbol");
                AddError($"Processing file {section.FileName} failed: {ex}");
            }
        }

        if (ValidationFailures.Count > 0)
        {
            await Send.ErrorsAsync(cancellation: ct);
        }
        else
        {
            await Send.OkAsync("Upload complete.", ct);
        }
    }
}