using FastEndpoints;

using Microsoft.AspNetCore.WebUtilities;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Allows uploading custom symbol files to the database.
/// </summary>
internal sealed class SymbolUploadEndpoint(DB db, ILogger<SymbolUploadEndpoint> logger, SymbolParsingService parsingService)
    : EndpointWithoutRequest
{
    /// <summary>
    /// Registers the HTTP POST endpoint at "/api/uploads/symbol", enables file uploads for the route, and applies the "Symbols" tag.
    /// </summary>
    public override void Configure()
    {
        Post("/api/uploads/symbol");
        AllowFileUploads(true);
        Options(x => x.WithTags("Symbols"));
        Policy(p => p.RequireAuthenticatedUser());
    }

    /// <summary>
    /// Processes uploaded symbol files from the HTTP request: parses each file, detects duplicates (honoring the optional "force" query parameter), creates or updates symbol entities in the database, uploads their blob data, and returns either validation errors or a completion response.
    /// </summary>
    /// <param name="ct">The cancellation token that can be used to cancel request processing.</param>
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
                if (string.IsNullOrWhiteSpace(section.FileName))
                {
                    logger.LogWarning("Encountered uploaded multipart section without a file name");
                    AddError("Uploaded multipart section has no file name.");
                    continue;
                }

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

                // duplicate check – single query by IndexPrefix, FileName, and SymbolKey (revision), then use result for count and existingSymbol
                List<SymbolsEntity> existingSymbols = await db.Find<SymbolsEntity>()
                    .ManyAsync(lr =>
                            lr.Eq(r => r.IndexPrefix, result.IndexPrefix) &
                            lr.Eq(r => r.FileName, result.FileName) &
                            lr.Eq(r => r.SymbolKey, result.SymbolKey)
                        , ct);

                if (existingSymbols.Count != 0)
                {
                    if (force.HasValue && force.Value)
                    {
                        existingSymbol = existingSymbols.FirstOrDefault();
                    }
                    else
                    {
                        logger.LogWarning("Symbol with name {Filename} and index prefix {IndexPrefix} already exists",
                            filename, result.IndexPrefix);
                        AddError(
                            $"Symbol with name {filename} and index prefix {result.IndexPrefix} already exists.");
                        continue;
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

                await db.SaveAsync(symbol, cancellation: ct);

                try
                {
                    await symbol.Data(db).UploadAsync(ms, cancellation: ct);
                }
                catch
                {
                    if (existingSymbol is null)
                    {
                        await db.DeleteAsync<SymbolsEntity>(symbol.ID, CancellationToken.None);
                    }
                    throw;
                }

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