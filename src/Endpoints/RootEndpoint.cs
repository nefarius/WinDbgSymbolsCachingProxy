using FastEndpoints;

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
        PeFile peFile = new(System.Reflection.Assembly.GetEntryAssembly()!.Location);

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
        
        await SendOkAsync(new RootResponse()
        {
            ServerVersion = version
        }, ct);
    }
}