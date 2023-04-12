using FastEndpoints;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public enum Badge
{
    CachedSymbolsTotal,
    CachedSymbolsNotFound,
    CachedSymbolsFound
}

public sealed class BadgeEndpoint : EndpointWithoutRequest
{
    private readonly ILogger<BadgeEndpoint> _logger;
    private readonly ISvgService _svgService;
    private readonly IOptions<ServiceConfig> _options;

    public BadgeEndpoint(ISvgService svgService, ILogger<BadgeEndpoint> logger, IOptions<ServiceConfig> options)
    {
        _svgService = svgService;
        _logger = logger;
        _options = options;
    }

    public override void Configure()
    {
        Get("/api/badges/{Name}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Badge badge = Route<Badge>("Name");

        using MemoryStream ms = new();

        BadgeParameters parameters = new();

        switch (badge)
        {
            case Badge.CachedSymbolsTotal:
                _logger.LogInformation("Returning cached symbols count");
                long symbolsCount = await DB.CountAsync<SymbolsEntity>(cancellation: ct);
                parameters.Label = "Cached Symbols Total";
                parameters.Result = symbolsCount.ToString();
                break;
            case Badge.CachedSymbolsNotFound:
                _logger.LogInformation("Returning cached 404 symbols count");
                long symbols404Count = await DB.CountAsync<SymbolsEntity>(
                    s => s.NotFoundAt != null,
                    cancellation: ct);
                parameters.Label = "Cached Symbols 404";
                parameters.Result = symbols404Count.ToString();
                break;
            case Badge.CachedSymbolsFound:
                _logger.LogInformation("Returning cached existing symbols count");
                long symbolsFoundCount =
                    await DB.CountAsync<SymbolsEntity>(
                        s => s.NotFoundAt == null,
                        cancellation: ct);
                parameters.Label = "Cached Symbols Found";
                parameters.Result = symbolsFoundCount.ToString();
                break;
            default:
                await SendNotFoundAsync(ct);
                return;
        }

        _svgService.Draw(parameters, ms);

        ms.Position = 0;

        DateTime now = DateTime.UtcNow;
        int expiresSeconds = _options.Value.BadgeExpiresSeconds;

        // cache control
        HttpContext.Response.Headers.CacheControl =
            new StringValues($"max-age={expiresSeconds}, s-maxage={expiresSeconds}");
        HttpContext.Response.Headers.Age = new StringValues("0");
        HttpContext.Response.Headers.Date = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.LastModified = new StringValues(now.ToString("R"));
        HttpContext.Response.Headers.Expires = new StringValues(now.AddSeconds(expiresSeconds).ToString("R"));

        await SendStreamAsync(ms, contentType: "image/svg+xml", cancellation: ct);
    }
}