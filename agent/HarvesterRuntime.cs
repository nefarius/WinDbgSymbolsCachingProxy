using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.FileSystemGlobbing;

using MimeTypes;

namespace HarvestingAgent;

public sealed class HarvesterRuntime : IDisposable
{
    private sealed class WatcherBinding
    {
        public required IReadOnlyList<ServerConfig> Servers { get; init; }
    }

    private sealed class UploadAttemptResult
    {
        public bool Success { get; init; }
        public bool ShouldDeleteAfterAllSuccess { get; init; }
        public string? ServerUrl { get; init; }
        public string? ErrorDetails { get; init; }
    }

    public static readonly string[] DefaultUploadFilters = ["*.exe", "*.dll", "*.sys", "*.pdb"];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HarvesterRuntime> _logger;
    private readonly AgentSettingsStore _store;
    private readonly HarvesterHealthState _health;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly object _gate = new();
    private Dictionary<FileSystemWatcher, WatcherBinding>? _maps;
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
        _maps = new Dictionary<FileSystemWatcher, WatcherBinding>();
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

    /// <summary>
    /// Rebuilds the set of file system watchers from the current configuration and publishes their runtime state.
    /// </summary>
    /// <remarks>
    /// Creates one watcher per unique normalized directory path and recursion setting, registers configured file filters
    /// and servers for each watcher, attaches Created event handlers, replaces any existing watchers atomically, updates
    /// the runtime health state, and disposes old watchers.
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token to abort the rebuild operation.</param>
    /// <exception cref="OperationCanceledException">Thrown if <paramref name="cancellationToken"/> is canceled.</exception>
    /// <exception cref="ObjectDisposedException">Thrown if the runtime has been disposed.</exception>
    public async Task RebuildWatchersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Yields so host startup cancellation is observed before taking the gate; Delay(0) honors the token.
        await Task.Delay(0, cancellationToken).ConfigureAwait(false);

        lock (_gate)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            AgentSettingsDocument doc = _store.GetSnapshot();
            ServiceConfig serviceConfig = doc.ToServiceConfig();
            Dictionary<FileSystemWatcher, WatcherBinding> newMaps = new();
            Dictionary<FileSystemWatcher, WatcherBinding> oldMaps = _maps ?? new Dictionary<FileSystemWatcher, WatcherBinding>();

            try
            {
                // Group servers by watcher path, then create one watcher per unique path.
                Dictionary<(string Path, bool Recursive), (HashSet<string> Filters, List<ServerConfig> Servers)> watcherGroups = new();

                foreach (ServerConfig serverConfig in serviceConfig.Servers)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (serverConfig.ServerUrl is null)
                    {
                        _logger.LogWarning("Skipping server entry with no ServerUrl");
                        continue;
                    }

                    IReadOnlyList<string> effectiveFilters = serverConfig.UploadFileFilters.Count > 0
                        ? serverConfig.UploadFileFilters
                        : DefaultUploadFilters;

                    foreach (WatcherPathEntry watch in serverConfig.WatcherPaths)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string path = PathNormalization.NormalizeDirectoryPath(watch.Path);
                        if (string.IsNullOrEmpty(path))
                        {
                            continue;
                        }

                        bool recursive = watch.IncludeSubdirectories;
                        (string Path, bool Recursive) key = (path, recursive);

                        if (!watcherGroups.TryGetValue(key, out var group))
                        {
                            group = (new HashSet<string>(StringComparer.OrdinalIgnoreCase), []);
                            watcherGroups[key] = group;
                        }

                        foreach (string pattern in effectiveFilters)
                        {
                            group.Filters.Add(pattern);
                        }

                        group.Servers.Add(serverConfig);
                    }
                }

                foreach (KeyValuePair<(string Path, bool Recursive), (HashSet<string> Filters, List<ServerConfig> Servers)> item in watcherGroups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    FileSystemWatcher? watcher = null;
                    try
                    {
                        watcher = new FileSystemWatcher(item.Key.Path)
                        {
                            NotifyFilter = NotifyFilters.Attributes
                                           | NotifyFilters.CreationTime
                                           | NotifyFilters.DirectoryName
                                           | NotifyFilters.FileName
                                           | NotifyFilters.LastWrite
                                           | NotifyFilters.Size,
                            IncludeSubdirectories = item.Key.Recursive
                        };

                        foreach (string pattern in item.Value.Filters)
                        {
                            watcher.Filters.Add(pattern);
                        }

                        _logger.LogInformation(
                            watcher.IncludeSubdirectories
                                ? "Watching over path {Path} ({@Filters}) and its subdirectories"
                                : "Watching over path {Path} ({@Filters})", watcher.Path,
                            watcher.Filters);

                        newMaps.Add(watcher, new WatcherBinding
                        {
                            Servers = item.Value.Servers.ToArray()
                        });
                        watcher = null;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Skipping invalid watcher path {Path}", item.Key.Path);
                        watcher?.Dispose();
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                bool enable = doc.HarvestingEnabled;
                List<WatcherStatusEntry> status = [];

                foreach (KeyValuePair<FileSystemWatcher, WatcherBinding> map in newMaps)
                {
                    cancellationToken.ThrowIfCancellationRequested();

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

                // Publish only after the new collection is fully initialized.
                _maps = newMaps;
                DisposeWatchersLocked(oldMaps);
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
            catch
            {
                DisposeWatchersLocked(newMaps);
                throw;
            }
        }
    }

    public Task SetHarvestingEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            _store.UpdateHarvestingEnabled(enabled);

            if (_maps is null)
            {
                return Task.CompletedTask;
            }

            List<WatcherStatusEntry> status = [];

            foreach (KeyValuePair<FileSystemWatcher, WatcherBinding> map in _maps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                FileSystemWatcher w = map.Key;
                w.EnableRaisingEvents = enabled;
                status.Add(new WatcherStatusEntry
                {
                    Path = w.Path,
                    Recursive = w.IncludeSubdirectories,
                    EventsEnabled = w.EnableRaisingEvents
                });
            }

            cancellationToken.ThrowIfCancellationRequested();
            _health.SetRuntimeState(enabled, status.AsReadOnly());

            cancellationToken.ThrowIfCancellationRequested();
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

        DisposeWatchersLocked(_maps);
    }

    private void DisposeWatchersLocked(Dictionary<FileSystemWatcher, WatcherBinding> maps)
    {
        int count = maps.Count;
        foreach (KeyValuePair<FileSystemWatcher, WatcherBinding> map in maps)
        {
            map.Key.Created -= WatcherOnCreated;
            map.Key.EnableRaisingEvents = false;
            map.Key.Dispose();
        }

        maps.Clear();
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
                WatcherBinding? binding;
                lock (_gate)
                {
                    if (_maps is null || !_maps.TryGetValue(watcher, out binding))
                    {
                        return;
                    }
                }

                if (binding.Servers.Count == 0)
                {
                    return;
                }

                string path = e.FullPath;
                string fileName = Path.GetFileName(path);
                _health.RecordFileDetected(path);

                await WaitForFileReadyAsync(path, stopping);

                string? mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));
                ArgumentException.ThrowIfNullOrEmpty(mimeType);

                Task<UploadAttemptResult>[] uploads = binding.Servers
                    .Select(serverConfig => UploadToServerAsync(serverConfig, path, fileName, mimeType, stopping))
                    .ToArray();

                UploadAttemptResult[] results = await Task.WhenAll(uploads);

                foreach (UploadAttemptResult result in results)
                {
                    if (!result.Success)
                    {
                        _health.RecordFileUploadFailure(
                            path,
                            result.ServerUrl,
                            result.ErrorDetails ?? "Upload failed");
                    }
                }

                bool allSucceeded = results.All(r => r.Success);
                bool shouldDelete = results.Any(r => r.ShouldDeleteAfterAllSuccess);

                if (allSucceeded && shouldDelete)
                {
                    File.Delete(path);
                    _logger.LogInformation("Symbol file {Symbol} deleted", path);
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload symbol {FullPath}", e.FullPath);
                _health.RecordFileUploadFailure(e.FullPath, null, ex.Message);
                _health.RecordError(ex.Message);
            }
        }, stopping);
    }

    private async Task<UploadAttemptResult> UploadToServerAsync(
        ServerConfig serverConfig,
        string path,
        string fileName,
        string mimeType,
        CancellationToken cancellationToken)
    {
        string? serverUrl = serverConfig.ServerUrl?.ToString();

        if (serverConfig.ServerUrl is null)
        {
            return new UploadAttemptResult
            {
                Success = false,
                ShouldDeleteAfterAllSuccess = false,
                ServerUrl = serverUrl,
                ErrorDetails = "Missing server URL"
            };
        }

        try
        {
            using HttpClient client = _httpClientFactory.CreateClient("Server");
            client.BaseAddress = serverConfig.ServerUrl;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{serverConfig.Authentication.Username}:{serverConfig.Authentication.Password}")));

            using MultipartFormDataContent form = new();
            await using FileStream fileStream = new(
                path,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1024 * 64,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            using StreamContent fileContent = new(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
            form.Add(fileContent, "symbol", fileName);

            using HttpResponseMessage response = await client.PostAsync(
                "/api/uploads/symbol?force=true",
                form,
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Symbol upload successful");
                _health.RecordUploadSuccess();
                _health.RecordFileUploadSuccess(path, serverUrl);

                bool shouldDelete = false;
                if (serverConfig.DeleteAfterUpload)
                {
                    Matcher matcher = new();
                    matcher.AddIncludePatterns(serverConfig.DeletionInclusionFilter);
                    matcher.AddExcludePatterns(serverConfig.DeletionExclusionFilter);

                    PatternMatchingResult match = matcher.Match(fileName);
                    shouldDelete = match.HasMatches;
                }

                return new UploadAttemptResult
                {
                    Success = true,
                    ShouldDeleteAfterAllSuccess = shouldDelete,
                    ServerUrl = serverUrl
                };
            }

            _logger.LogInformation("Symbol upload failed");
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            _health.RecordUploadFailure($"HTTP {(int)response.StatusCode}: {body}");
            await File.WriteAllTextAsync($"{path}.upload-error.txt", body, cancellationToken);
            return new UploadAttemptResult
            {
                Success = false,
                ShouldDeleteAfterAllSuccess = false,
                ServerUrl = serverUrl,
                ErrorDetails = $"HTTP {(int)response.StatusCode}: {body}"
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Symbol upload exception for {ServerUrl}", serverUrl);
            _health.RecordUploadFailure(ex.Message);
            return new UploadAttemptResult
            {
                Success = false,
                ShouldDeleteAfterAllSuccess = false,
                ServerUrl = serverUrl,
                ErrorDetails = ex.Message
            };
        }
    }
}
