using System.Reflection;

using FastEndpoints;

using Microsoft.Extensions.Caching.Memory;

using MongoDB.Entities;

using PeNet;
using PeNet.Header.Resource;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class InfoEndpoint(ILogger<InfoEndpoint> logger, IMemoryCache memoryCache)
    : EndpointWithoutRequest<RootResponse>
{
    public override void Configure()
    {
        Get("/info");
        AllowAnonymous();
        Options(builder => builder.ExcludeFromDescription());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (memoryCache.TryGetValue(nameof(InfoEndpoint), out RootResponse? response) && response is not null)
        {
            await Send.OkAsync(response, ct);
            return;
        }

        PeFile peFile = new(Assembly.GetEntryAssembly()!.Location);

        if (peFile.Resources is null)
        {
            logger.LogError("Couldn't get PE file resources");
            await Send.ErrorsAsync(500, ct);
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

        memoryCache.Set(nameof(InfoEndpoint), response, TimeSpan.FromHours(1));

        await Send.OkAsync(response, ct);
    }
}