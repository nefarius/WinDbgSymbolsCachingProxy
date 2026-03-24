using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.FileSystemGlobbing;

using MimeTypes;

namespace HarvestingAgent;

public sealed class HarvesterRuntime : IDisposable
{
    public static readonly string[] DefaultUploadFilters = ["*.exe", "*.dll", "*.sys", "*.pdb"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HarvesterRuntime> _logger;
    private readonly AgentSettingsStore _store;
    private readonly HarvesterHealthState _health;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly object _gate = new();
    private Dictionary<FileSystemWatcher, ServerConfig>? _maps;
    private bool _disposed;

    public HarvesterRuntime(
        IHttpClientFactory httpClientFactory,
        ILogger<HarvesterRuntime> logger,
        AgentSettingsStore store,
        HarvesterHealthState health,
        IHostApplicationLifetime lifetime)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _store = store;
        _health = health;
        _lifetime = lifetime;
        _maps = new Dictionary<FileSystemWatcher, ServerConfig>();
    }

    /// <summary>
    ///     Waits until the file can be opened for exclusive read or cancellation is requested.
    /// </summary>
    private static async Task WaitForFileReadyAsync(string fullPath, CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using FileStream stream = new(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.None,
                    bufferSize: 1,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);

                break;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (DirectoryNotFoundException)
            {
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (IOException)
            {
                await Task.Delay(500, cancellationToken);
            }
            catch (Exception)
            {
                await Task.Delay(500, cancellationToken);
            }
        }
    }

    public Task RebuildWatchersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            DisposeWatchersLocked();

            AgentSettingsDocument doc = _store.GetSnapshot();
            ServiceConfig serviceConfig = doc.ToServiceConfig();
            Dictionary<FileSystemWatcher, ServerConfig>? maps = new Dictionary<FileSystemWatcher, ServerConfig>();

            try
            {
                foreach (ServerConfig serverConfig in serviceConfig.Servers)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (serverConfig.ServerUrl is null)
                    {
                        _logger.LogWarning("Skipping server entry with no ServerUrl");
                        continue;
                    }

                    IReadOnlyList<string> filters = serverConfig.UploadFileFilters.Count > 0
                        ? serverConfig.UploadFileFilters
                        : DefaultUploadFilters;

                    foreach (WatcherPathEntry watch in serverConfig.WatcherPaths)
                    {
                        string path = (watch.Path ?? string.Empty).Trim();
                        if (string.IsNullOrEmpty(path))
                        {
                            continue;
                        }

                        bool recursive = watch.IncludeSubdirectories;
                        FileSystemWatcher? watcher = null;
                        try
                        {
                            watcher = new FileSystemWatcher(path)
                            {
                                NotifyFilter = NotifyFilters.Attributes
                                               | NotifyFilters.CreationTime
                                               | NotifyFilters.DirectoryName
                                               | NotifyFilters.FileName
                                               | NotifyFilters.LastWrite
                                               | NotifyFilters.Size,
                                IncludeSubdirectories = recursive
                            };

                            foreach (string pattern in filters)
                            {
                                watcher.Filters.Add(pattern);
                            }

                            _logger.LogInformation(
                                watcher.IncludeSubdirectories
                                    ? "Watching over path {Path} ({@Filters}) and its subdirectories"
                                    : "Watching over path {Path} ({@Filters})", watcher.Path,
                                watcher.Filters);

                            maps.Add(watcher, serverConfig);
                            watcher = null;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Skipping invalid watcher path {Path}", path);
                            watcher?.Dispose();
                        }
                    }
                }

                _maps = maps;
                maps = null;

                bool enable = doc.HarvestingEnabled;
                List<WatcherStatusEntry> status = [];

                foreach (KeyValuePair<FileSystemWatcher, ServerConfig> map in _maps)
                {
                    FileSystemWatcher w = map.Key;
                    w.Created += WatcherOnCreated;
                    w.EnableRaisingEvents = enable;
                    status.Add(new WatcherStatusEntry
                    {
                        Path = w.Path,
                        Recursive = w.IncludeSubdirectories,
                        EventsEnabled = w.EnableRaisingEvents
                    });
                }

                _health.SetRuntimeState(enable, status.AsReadOnly());

                if (enable)
                {
                    _logger.LogInformation("Watchers started");
                }
                else
                {
                    _logger.LogInformation("Watchers created but harvesting is disabled");
                }
            }
            finally
            {
                if (maps is not null)
                {
                    foreach (FileSystemWatcher w in maps.Keys)
                    {
                        try
                        {
                            w.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogDebug(ex, "Error disposing watcher during failed rebuild");
                        }
                    }

                    maps.Clear();
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task SetHarvestingEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            _store.UpdateHarvestingEnabled(enabled);

            if (_maps is null)
            {
                return Task.CompletedTask;
            }

            List<WatcherStatusEntry> status = [];

            foreach (KeyValuePair<FileSystemWatcher, ServerConfig> map in _maps)
            {
                FileSystemWatcher w = map.Key;
                w.EnableRaisingEvents = enabled;
                status.Add(new WatcherStatusEntry
                {
                    Path = w.Path,
                    Recursive = w.IncludeSubdirectories,
                    EventsEnabled = w.EnableRaisingEvents
                });
            }

            _health.SetRuntimeState(enabled, status.AsReadOnly());

            _logger.LogInformation(enabled ? "Watchers enabled" : "Watchers disabled");
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            DisposeWatchersLocked();
            _maps = null;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void DisposeWatchersLocked()
    {
        if (_maps is null)
        {
            return;
        }

        int count = _maps.Count;
        foreach (KeyValuePair<FileSystemWatcher, ServerConfig> map in _maps)
        {
            map.Key.Created -= WatcherOnCreated;
            map.Key.EnableRaisingEvents = false;
            map.Key.Dispose();
        }

        _maps.Clear();
        if (count > 0)
        {
            _logger.LogInformation("Watchers stopped");
        }
    }

    private void WatcherOnCreated(object sender, FileSystemEventArgs e)
    {
        FileSystemWatcher? watcher = sender as FileSystemWatcher;
        ArgumentNullException.ThrowIfNull(watcher);

        CancellationToken stopping = _lifetime.ApplicationStopping;

        Task.Run(async () =>
        {
            try
            {
                ServerConfig? serverConfig;
                lock (_gate)
                {
                    if (_maps is null || !_maps.TryGetValue(watcher, out serverConfig))
                    {
                        return;
                    }
                }

                if (serverConfig.ServerUrl is null)
                {
                    return;
                }

                using HttpClient client = _httpClientFactory.CreateClient("Server");

                client.BaseAddress = serverConfig.ServerUrl;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{serverConfig.Authentication.Username}:{serverConfig.Authentication.Password}")));

                using MultipartFormDataContent form = new();
                string path = e.FullPath;

                await WaitForFileReadyAsync(path, stopping);

                ByteArrayContent fileContent = new(await File.ReadAllBytesAsync(path, stopping));
                string? mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                ArgumentException.ThrowIfNullOrEmpty(mimeType);

                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                form.Add(fileContent, "symbol", Path.GetFileName(path));

                using HttpResponseMessage response = await client.PostAsync(
                    "/api/uploads/symbol?force=true",
                    form,
                    stopping);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Symbol upload successful");
                    _health.RecordUploadSuccess();

                    if (serverConfig.DeleteAfterUpload)
                    {
                        Matcher matcher = new();
                        matcher.AddIncludePatterns(serverConfig.DeletionInclusionFilter);
                        matcher.AddExcludePatterns(serverConfig.DeletionExclusionFilter);

                        PatternMatchingResult match = matcher.Match(Path.GetFileName(path));

                        if (match.HasMatches)
                        {
                            File.Delete(path);
                            _logger.LogInformation("Symbol file {Symbol} deleted", path);
                        }
                        else
                        {
                            _logger.LogInformation("Symbol file {Symbol} excluded from deletion", path);
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("Symbol upload failed");
                    string body = await response.Content.ReadAsStringAsync(stopping);
                    _health.RecordUploadFailure($"HTTP {(int)response.StatusCode}: {body}");
                    await File.WriteAllTextAsync($"{path}.upload-error.txt", body, stopping);
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload symbol {FullPath}", e.FullPath);
                _health.RecordError(ex.Message);
            }
        }, stopping);
    }
}
