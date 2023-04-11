using FastEndpoints;

using MongoDB.Entities;

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

        await SendStreamAsync(ms, contentType: "image/svg+xml", cancellation: ct);
    }
}