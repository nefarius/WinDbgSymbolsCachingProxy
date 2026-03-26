using JetBrains.Annotations;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

using MongoDB.Entities;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Models;

namespace WinDbgSymbolsCachingProxy.Components.Pages;

[UsedImplicitly]
public partial class Status : IDisposable
{
    private const string OgImageWidthPx = "1200";
    private const string OgImageHeightPx = "630";
    private const string PageTitleText = "Symbols Server — Status";
    private const string PageHeadingText = "Database status";

    [Inject]
    private DB Db { get; set; } = null!;

    [Inject]
    private ILogger<Status> Logger { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    private string OgAbsoluteBase => NavigationManager.BaseUri.TrimEnd('/');

    private string CanonicalStatusUrl => $"{OgAbsoluteBase}/status";

    private string OpenGraphImageUrl => $"{OgAbsoluteBase}/og/status.png";

    private string OgDescription =>
        _loadFailed
            ? "Live database status for this WinDbg symbols caching proxy."
            : $"Cached symbols: {_cachedTotal:N0} total · {_cachedFound:N0} found · {_cachedNotFound:N0} recorded as not found upstream.";

    private CancellationTokenSource? _cts;
    private bool _loading = true;
    private bool _loadFailed;
    private long _cachedTotal;
    private long _cachedFound;
    private long _cachedNotFound;
    private string? _serverVersion;

    protected override async Task OnInitializedAsync()
    {
        _cts = new CancellationTokenSource();
        CancellationToken ct = _cts.Token;

        try
        {
            _serverVersion = ApplicationVersionHelper.TryGetServerVersion();

            Task<long> totalTask = Db.CountAsync<SymbolsEntity>(cancellation: ct);
            Task<long> notFoundTask = Db.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt != null,
                cancellation: ct);
            Task<long> foundTask = Db.CountAsync<SymbolsEntity>(
                s => s.NotFoundAt == null,
                cancellation: ct);

            await Task.WhenAll(totalTask, notFoundTask, foundTask);

            _cachedTotal = await totalTask;
            _cachedNotFound = await notFoundTask;
            _cachedFound = await foundTask;
        }
        catch (OperationCanceledException)
        {
            // Component disposed during load
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed loading status");
            _loadFailed = true;
        }
        finally
        {
            _loading = false;
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
