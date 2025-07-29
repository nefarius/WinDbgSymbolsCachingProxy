using FastEndpoints;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

/// <summary>
///     Available badge types.
/// </summary>
public enum Badge
{
    /// <summary>
    ///     The total number of cached symbols.
    /// </summary>
    CachedSymbolsTotal,

    /// <summary>
    ///     The total number of symbols marked as not-found.
    /// </summary>
    CachedSymbolsNotFound,

    /// <summary>
    ///     The total number of cached symbols with downloadable content.
    /// </summary>
    CachedSymbolsFound
}

/// <summary>
///     Serves certain database metrics as embeddable SVG badges.
/// </summary>
public sealed class BadgeEndpoint(
    ISvgService svgService,
    ILogger<BadgeEndpoint> logger,
    IOptions<ServiceConfig> options)
    : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/badges/{Name}");
        AllowAnonymous();
        Options(x => x.WithTags("Badges"));
        Summary(s =>
        {
            s.Summary = "Serves SVG badges";
            s.Description = "Serves certain database metrics as embeddable SVG badges";
            s.Params["Name"] = "Name of the badge to generate";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Badge badge = Route<Badge>("Name");

        using MemoryStream ms = new();

        BadgeParameters parameters = new();

        switch (badge)
        {
            case Badge.CachedSymbolsTotal:
                logger.LogDebug("Returning cached symbols count");
                long symbolsCount = await DB.CountAsync<SymbolsEntity>(cancellation: ct);
                parameters.Label = "Cached Symbols Total";
                parameters.Result = symbolsCount.ToString();
                parameters.ResultColor = "#0f82bfff"; // blue
                break;
            case Badge.CachedSymbolsNotFound:
                logger.LogDebug("Returning cached 404 symbols count");
                long symbols404Count = await DB.CountAsync<SymbolsEntity>(
                    s => s.NotFoundAt != null,
                    cancellation: ct);
                parameters.Label = "Cached Symbols 404";
                parameters.Result = symbols404Count.ToString();
                parameters.ResultColor = "#dcb135ff"; // yellow
                break;
            case Badge.CachedSymbolsFound:
                logger.LogDebug("Returning cached existing symbols count");
                long symbolsFoundCount =
                    await DB.CountAsync<SymbolsEntity>(
                        s => s.NotFoundAt == null,
                        cancellation: ct);
                parameters.Label = "Cached Symbols Found";
                parameters.Result = symbolsFoundCount.ToString();
                parameters.ResultColor = "#2ac33bff"; // green
                break;
            default:
                await Send.NotFoundAsync(ct);
                return;
        }

        svgService.Draw(parameters, ms);

        ms.Position = 0;

        DateTime now = DateTime.UtcNow;
        int expiresSeconds = options.Value.BadgeExpiresSeconds;

        // cache control, otherwise GitHub etc. will not request an update for like a day or so
        HttpContext.Response.Headers.CacheControl =
            new StringValues($"max-age={expiresSeconds}, s-maxage={expiresSeconds}");
        HttpContext.Response.Headers.Age = new StringValues("0");
        HttpContext.Response.Headers.Date = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.LastModified = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.Expires = new StringValues(now.AddSeconds(expiresSeconds).ToString("R"));

        await Send.StreamAsync(ms, contentType: "image/svg+xml", cancellation: ct);
    }
}