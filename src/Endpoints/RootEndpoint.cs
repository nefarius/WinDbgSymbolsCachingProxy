﻿using System.Reflection;

using FastEndpoints;

using MongoDB.Entities;

using PeNet;
using PeNet.Header.Resource;

using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Endpoints;

public sealed class RootEndpoint : EndpointWithoutRequest<RootResponse>
{
    private readonly ILogger<RootEndpoint> _logger;

    public RootEndpoint(ILogger<RootEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        PeFile peFile = new(Assembly.GetEntryAssembly()!.Location);

        if (peFile.Resources is null)
        {
            _logger.LogError("Couldn't get PE file resources");
            await SendErrorsAsync(500, ct);
            return;
        }
        
        StringTable stringTable = peFile.Resources.VsVersionInfo!.StringFileInfo.StringTable.First();

        string? productVersion = stringTable.ProductVersion;

        if (productVersion is null)
        {
            _logger.LogError("Couldn't get product version");
            await SendErrorsAsync(500, ct);
            return;
        }

        Version version = System.Version.Parse(productVersion);

        await SendOkAsync(new RootResponse
        {
            ServerVersion = version,
            CachedSymbolsTotal = await DB.CountAsync<SymbolsEntity>(cancellation: ct),
            CachedSymbols404 = await DB.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt != null,
                cancellation: ct),
            CachedSymbolsFound = await DB.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt == null,
                cancellation: ct)
        }, ct);
    }
}