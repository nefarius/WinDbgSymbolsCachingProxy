using System.Reflection;

using FastEndpoints;

using Microsoft.Extensions.Caching.Memory;

using MongoDB.Entities;

using PeNet;
using PeNet.Header.Resource;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class InfoEndpoint : EndpointWithoutRequest<RootResponse>
{
    private readonly ILogger<InfoEndpoint> _logger;
    private readonly IMemoryCache _memoryCache;

    public InfoEndpoint(ILogger<InfoEndpoint> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public override void Configure()
    {
        Get("/info");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (_memoryCache.TryGetValue(nameof(InfoEndpoint), out RootResponse? response) && response is not null)
        {
            await SendOkAsync(response, ct);
            return;
        }

        PeFile peFile = new(Assembly.GetEntryAssembly()!.Location);

        if (peFile.Resources is null)
        {
            _logger.LogError("Couldn't get PE file resources");
            await SendErrorsAsync(500, ct);
            return;
        }

        StringTable stringTable = peFile.Resources.VsVersionInfo!.StringFileInfo.StringTable.First();

        response = new RootResponse
        {
            ServerVersion = stringTable.FileVersion,
            CachedSymbolsTotal = await DB.CountAsync<SymbolsEntity>(cancellation: ct),
            CachedSymbols404 = await DB.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt != null,
                cancellation: ct),
            CachedSymbolsFound = await DB.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt == null,
                cancellation: ct)
        };

        _memoryCache.Set(nameof(InfoEndpoint), response, TimeSpan.FromHours(1));

        await SendOkAsync(response, ct);
    }
}