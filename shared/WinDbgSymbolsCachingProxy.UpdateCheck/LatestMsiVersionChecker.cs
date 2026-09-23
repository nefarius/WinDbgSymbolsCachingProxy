using System.Net.Http.Json;

namespace WinDbgSymbolsCachingProxy.UpdateCheck;

/// <summary>
///     Compares the locally installed MSI, or the running assembly when no MSI is installed,
///     with the latest published MSI metadata.
/// </summary>
public sealed class LatestMsiVersionChecker
{
    public const string HttpClientName = "LatestMsiMetadata";

    public static readonly Uri MetadataUri = new(
        "https://buildbot.nefarius.at/builds/WinDbgSymbolsCachingProxy/latest/.WinDbgSymbolsCachingProxy.msi.json");

    public static readonly Uri DownloadUri = new(
        "https://buildbot.nefarius.at/builds/WinDbgSymbolsCachingProxy/latest/WinDbgSymbolsCachingProxy.msi");

    public static readonly TimeSpan DefaultCacheLifetime = TimeSpan.FromHours(1);

    private readonly HttpClient _httpClient;
    private readonly IInstalledMsiVersionSource _installedMsiVersions;
    private readonly ILocalAssemblyVersionSource _assemblyVersions;
    private readonly TimeProvider _timeProvider;
    private readonly TimeSpan _cacheLifetime;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly object _cacheGate = new();
    private VersionCheckStatus? _cached;
    private DateTimeOffset _cachedAt;

    public LatestMsiVersionChecker(
        HttpClient httpClient,
        IInstalledMsiVersionSource installedMsiVersions,
        ILocalAssemblyVersionSource assemblyVersions,
        TimeProvider? timeProvider = null,
        TimeSpan? cacheLifetime = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(installedMsiVersions);
        ArgumentNullException.ThrowIfNull(assemblyVersions);
        if (cacheLifetime is { } lifetime && lifetime <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(cacheLifetime));

        _httpClient = httpClient;
        _installedMsiVersions = installedMsiVersions;
        _assemblyVersions = assemblyVersions;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _cacheLifetime = cacheLifetime ?? DefaultCacheLifetime;
    }

    /// <summary>
    ///     Returns the local version without contacting the build server.
    /// </summary>
    public VersionCheckStatus GetLocalStatus()
    {
        (string? localVersion, LocalVersionOrigin origin) = ResolveLocal();
        return LocalOnly(localVersion, origin);
    }

    /// <summary>
    ///     Returns the cached comparison, fetching metadata when the cache is empty or expired.
    ///     Network, JSON, and registry failures leave the local version visible and do not report an update.
    /// </summary>
    public async Task<VersionCheckStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        VersionCheckStatus? cached = TryGetCached();
        if (cached is not null)
            return cached;

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            cached = TryGetCached();
            if (cached is not null)
                return cached;

            VersionCheckStatus status = await FetchAsync(cancellationToken).ConfigureAwait(false);
            Store(status);
            return status;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private async Task<VersionCheckStatus> FetchAsync(CancellationToken cancellationToken)
    {
        (string? localVersion, LocalVersionOrigin origin) = ResolveLocal();
        try
        {
            using HttpResponseMessage response = await _httpClient
                .GetAsync(MetadataUri, cancellationToken)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            LatestMsiMetadata? metadata = await response.Content
                .ReadFromJsonAsync<LatestMsiMetadata>(cancellationToken)
                .ConfigureAwait(false);
            if (metadata is null ||
                !string.Equals(metadata.Type, "msi", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(metadata.ProductVersion))
                return LocalOnly(localVersion, origin);

            string latestVersion = metadata.ProductVersion.Trim();
            bool updateAvailable = ProductVersionComparer.IsNewer(latestVersion, localVersion);
            return new VersionCheckStatus(
                localVersion,
                origin,
                latestVersion,
                updateAvailable,
                updateAvailable ? DownloadUri : null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return LocalOnly(localVersion, origin);
        }
    }

    private (string? Version, LocalVersionOrigin Origin) ResolveLocal()
    {
        string? installed = TryReadInstalledVersion();
        if (installed is not null && ProductVersionComparer.TryParse(installed, out _))
            return (installed, LocalVersionOrigin.InstalledMsi);

        string? assemblyVersion = TryReadAssemblyVersion();
        if (!string.IsNullOrWhiteSpace(assemblyVersion))
            return (assemblyVersion.Trim(), LocalVersionOrigin.Assembly);

        return (null, LocalVersionOrigin.None);
    }

    private string? TryReadInstalledVersion()
    {
        try
        {
            string? installed = _installedMsiVersions.TryGetVersion();
            return string.IsNullOrWhiteSpace(installed) ? null : installed.Trim();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private string? TryReadAssemblyVersion()
    {
        try
        {
            return _assemblyVersions.TryGetVersion();
        }
        catch (Exception)
        {
            return null;
        }
    }

    private VersionCheckStatus? TryGetCached()
    {
        lock (_cacheGate)
        {
            if (_cached is null)
                return null;

            TimeSpan age = _timeProvider.GetUtcNow() - _cachedAt;
            if (age < TimeSpan.Zero || age >= _cacheLifetime)
                return null;

            return _cached;
        }
    }

    private void Store(VersionCheckStatus status)
    {
        lock (_cacheGate)
        {
            _cached = status;
            _cachedAt = _timeProvider.GetUtcNow();
        }
    }

    private static VersionCheckStatus LocalOnly(string? localVersion, LocalVersionOrigin origin)
    {
        return new VersionCheckStatus(localVersion, origin, null, false, null);
    }
}
