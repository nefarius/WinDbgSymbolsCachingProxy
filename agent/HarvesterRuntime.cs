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
        public string? ServerDisplayName { get; init; }
        public string? ServerUrl { get; init; }
        public string? ErrorDetails { get; init; }
    }

    /// <summary>
    ///     Identifies a concrete on-disk revision of a symbol file (length + last write UTC).
    /// </summary>
    private readonly record struct SymbolFileVersion(long Length, DateTime LastWriteTimeUtc);

    public static readonly string[] DefaultUploadFilters = ["*.exe", "*.dll", "*.sys", "*.pdb"];

    /// <summary>
    ///     Share flags for reading build outputs while MSBuild or the linker may still have the file open.
    /// </summary>
    private const FileShare SymbolFileShare = FileShare.Read | FileShare.Write | FileShare.Delete;

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

    private static SymbolFileVersion GetSymbolFileVersion(string fullPath)
    {
        FileInfo fi = new(fullPath);
        return new SymbolFileVersion(fi.Length, fi.LastWriteTimeUtc);
    }

    /// <summary>
    ///     Waits until the file size is stable and it can be opened for read with sharing compatible with live MSBuild output.
    ///     Returns the file version observed after that probe (length + last write time).
    /// </summary>
    private static async Task<SymbolFileVersion> WaitForFileReadyAsync(string fullPath,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                long len1 = new FileInfo(fullPath).Length;
                await Task.Delay(150, cancellationToken);
                long len2 = new FileInfo(fullPath).Length;
                if (len1 != len2)
                {
                    await Task.Delay(350, cancellationToken);
                    continue;
                }

                await using FileStream stream = new(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    SymbolFileShare,
                    bufferSize: 1,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);

                return GetSymbolFileVersion(fullPath);
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
    ///     Copies the symbol to a temp file using a short-lived shared read on the source, so uploads do not hold handles on build outputs.
    ///     Returns the temp path and the source file version that was snapshotted (length + last write UTC at copy start).
    /// </summary>
    private async Task<(string TempPath, SymbolFileVersion Version)> CreateUploadSnapshotAsync(string sourcePath,
        SymbolFileVersion versionAfterWait,
        CancellationToken cancellationToken)
    {
        string tempName = $"harvester-{Guid.NewGuid():N}-{Path.GetFileName(sourcePath)}";
        string tempPath = Path.Combine(Path.GetTempPath(), tempName);

        SymbolFileVersion versionBeforeOpen = GetSymbolFileVersion(sourcePath);
        if (versionBeforeOpen != versionAfterWait)
        {
            _logger.LogWarning(
                "Symbol file {Path} changed between ready check and snapshot start (wait token {WaitVersion}, now {CurrentVersion})",
                sourcePath,
                versionAfterWait,
                versionBeforeOpen);
        }

        bool copySucceeded = false;
        try
        {
            await using FileStream source = new(
                sourcePath,
                FileMode.Open,
                FileAccess.Read,
                SymbolFileShare,
                bufferSize: 1024 * 64,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan);
            SymbolFileVersion snapshottedVersion = GetSymbolFileVersion(sourcePath);

            await using FileStream dest = new(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1024 * 64,
                options: FileOptions.Asynchronous);

            await source.CopyToAsync(dest, cancellationToken);
            copySucceeded = true;
            return (tempPath, snapshottedVersion);
        }
        finally
        {
            if (!copySucceeded && File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // best-effort cleanup of partial temp file
                }
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

                SymbolFileVersion versionAfterWait = await WaitForFileReadyAsync(path, stopping);

                string? snapshotPath = null;
                SymbolFileVersion snapshottedVersion = default;
                try
                {
                    (snapshotPath, snapshottedVersion) =
                        await CreateUploadSnapshotAsync(path, versionAfterWait, stopping);

                    string? mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));
                    ArgumentException.ThrowIfNullOrEmpty(mimeType);

                    Task<UploadAttemptResult>[] uploads = binding.Servers
                        .Select(serverConfig =>
                            UploadToServerAsync(
                                serverConfig,
                                snapshotPath,
                                path,
                                fileName,
                                mimeType,
                                snapshottedVersion,
                                stopping))
                        .ToArray();

                    UploadAttemptResult[] results = await Task.WhenAll(uploads);

                    foreach (UploadAttemptResult result in results)
                    {
                        if (!result.Success)
                        {
                            _health.RecordFileUploadFailure(
                                path,
                                result.ServerDisplayName,
                                result.ServerUrl,
                                result.ErrorDetails ?? "Upload failed");
                        }
                    }

                    bool allSucceeded = results.All(r => r.Success);
                    bool shouldDelete = results.Any(r => r.ShouldDeleteAfterAllSuccess);

                    if (allSucceeded && shouldDelete)
                    {
                        try
                        {
                            SymbolFileVersion currentVersion = GetSymbolFileVersion(path);
                            if (currentVersion != snapshottedVersion)
                            {
                                _logger.LogWarning(
                                    "Not deleting {Path}: file changed after snapshot (snapshotted version {Snapshotted}, current {Current})",
                                    path,
                                    snapshottedVersion,
                                    currentVersion);
                            }
                            else
                            {
                                File.Delete(path);
                                _logger.LogInformation("Symbol file {Symbol} deleted", path);
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            _logger.LogWarning("Not deleting {Path}: file no longer exists", path);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Could not verify or delete {Path} after upload", path);
                        }
                    }
                }
                finally
                {
                    if (snapshotPath is not null)
                    {
                        try
                        {
                            File.Delete(snapshotPath);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete temp upload snapshot {TempPath}", snapshotPath);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload symbol {FullPath}", e.FullPath);
                _health.RecordFileUploadFailure(e.FullPath, null, null, ex.Message);
                _health.RecordError(ex.Message);
            }
        }, stopping);
    }

    private async Task<UploadAttemptResult> UploadToServerAsync(
        ServerConfig serverConfig,
        string snapshotPath,
        string detectedPath,
        string fileName,
        string mimeType,
        SymbolFileVersion snapshottedSourceVersion,
        CancellationToken cancellationToken)
    {
        string? serverUrl = serverConfig.ServerUrl?.ToString();
        string? serverDisplayName = string.IsNullOrWhiteSpace(serverConfig.DisplayName)
            ? null
            : serverConfig.DisplayName.Trim();

        if (serverConfig.ServerUrl is null)
        {
            return new UploadAttemptResult
            {
                Success = false,
                ShouldDeleteAfterAllSuccess = false,
                ServerDisplayName = serverDisplayName,
                ServerUrl = serverUrl,
                ErrorDetails = "Missing server URL"
            };
        }

        try
        {
            _logger.LogTrace(
                "Upload {FileName} from snapshot for {Path} (snapshotted revision {Version})",
                fileName,
                detectedPath,
                snapshottedSourceVersion);

            using HttpClient client = _httpClientFactory.CreateClient("Server");
            client.BaseAddress = serverConfig.ServerUrl;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Basic",
                Convert.ToBase64String(
                    Encoding.UTF8.GetBytes(
                        $"{serverConfig.Authentication.Username}:{serverConfig.Authentication.Password}")));

            using MultipartFormDataContent form = new();
            await using FileStream fileStream = new(
                snapshotPath,
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
                _health.RecordFileUploadSuccess(detectedPath, serverDisplayName, serverUrl);

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
                    ServerDisplayName = serverDisplayName,
                    ServerUrl = serverUrl
                };
            }

            _logger.LogInformation("Symbol upload failed");
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            _health.RecordUploadFailure($"HTTP {(int)response.StatusCode}: {body}");
            await File.WriteAllTextAsync($"{detectedPath}.upload-error.txt", body, cancellationToken);
            return new UploadAttemptResult
            {
                Success = false,
                ShouldDeleteAfterAllSuccess = false,
                ServerDisplayName = serverDisplayName,
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
                ServerDisplayName = serverDisplayName,
                ServerUrl = serverUrl,
                ErrorDetails = ex.Message
            };
        }
    }
}
