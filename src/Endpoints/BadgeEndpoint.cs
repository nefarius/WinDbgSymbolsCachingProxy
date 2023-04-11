using FastEndpoints;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public enum Badge
{
    CachedSymbolsTotal
}

public sealed class BadgeEndpoint : EndpointWithoutRequest
{
    private readonly ISvgService _svgService;
    private readonly ILogger<BadgeEndpoint> _logger;

    public BadgeEndpoint(ISvgService svgService, ILogger<BadgeEndpoint> logger)
    {
        _svgService = svgService;
        _logger = logger;
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
                _logger.LogInformation("Returning cached symbol count");
                long symbolsCount = await DB.CountAsync<SymbolsEntity>(cancellation: ct);
                parameters.Label = "Cached Symbols";
                parameters.Result = symbolsCount.ToString();
                break;
            default:
                await SendNotFoundAsync(ct);
                return;
        }

        _svgService.Draw(parameters, ms);

        ms.Position = 0;

        await SendStreamAsync(ms, contentType: "image/svg+xml", cancellation: ct);
    }
}