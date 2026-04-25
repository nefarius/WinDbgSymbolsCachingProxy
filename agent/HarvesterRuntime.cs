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
        public required IReadOnlyList<WatcherPathEntry> WatchRules { get; init; }
    }

    private sealed class PendingFileEvent
    {
        public required string Path { get; init; }
        public required string FileName { get; init; }
        public required List<WatcherBinding> Bindings { get; init; }
        public required long WatcherSetGeneration { get; init; }
        public required CancellationToken WatcherSetCancellationToken { get; init; }
        public int Revision { get; set; }
    }

    private sealed class UploadAttemptResult
    {
        public bool Success { get; init; }
        public string? ServerDisplayName { get; init; }
        public string? ServerUrl { get; init; }
        public string? ErrorDetails { get; init; }
    }

    /// <summary>
    ///     Identifies a concrete on-disk revision of a symbol file (length + last write UTC).
    /// </summary>
    private readonly record struct SymbolFileVersion(long Length, DateTime LastWriteTimeUtc);

    /// <summary>
    ///     Share flags for reading build outputs while MSBuild or the linker may still have the file open.
    /// </summary>
    private const FileShare SymbolFileShare = FileShare.Read | FileShare.Write | FileShare.Delete;

    /// <summary>
    ///     Number of consecutive stable probes required before considering a symbol file fully written.
    ///     With <see cref="StabilityProbeInterval" /> set to 500 ms this translates to a minimum
    ///     quiescent window of ~2 s, which tolerates the write-burst pauses typical of SMB/NFS copies
    ///     from remote locations where TCP stalls, Nagle batching, or receiver backpressure can produce
    ///     multi-hundred-millisecond gaps between writes on the destination.
    /// </summary>
    private const int StabilityRequiredConsecutiveSamples = 5;

    /// <summary>
    ///     Delay between stability probes. Longer than a typical local disk flush cycle so network
    ///     copies don't get interpreted as "done" between bursts.
    /// </summary>
    private static readonly TimeSpan StabilityProbeInterval = TimeSpan.FromMilliseconds(500);

    /// <summary>
    ///     Upper bound for how long to wait for a symbol file to reach a stable, valid state before
    ///     giving up. Sized to accommodate multi-hundred-MB PDBs being copied in from slow network
    ///     shares where end-to-end transfer can take many minutes.
    /// </summary>
    private static readonly TimeSpan FileReadyTimeout = TimeSpan.FromMinutes(15);

    /// <summary>
    ///     After the file metadata has been stable long enough, we additionally try to open the file
    ///     without granting write-share access to confirm no writer still holds a handle on it. On a
    ///     slow copy that check keeps failing while the copier is flushing the tail of the file; this
    ///     is the maximum time we will insist on it before falling back to the stability signal alone
    ///     (handles the case of an IDE/AV that keeps a PDB open for its own reasons).
    /// </summary>
    private static readonly TimeSpan WriterLockProbeGracePeriod = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Maximum number of snapshot attempts when the source file keeps changing during the copy.
    /// </summary>
    private const int SnapshotMaxAttempts = 5;

    /// <summary>
    ///     Coalesces the burst of change notifications emitted while compilers and linkers are writing build outputs.
    /// </summary>
    private static readonly TimeSpan FileEventDebounceInterval = TimeSpan.FromSeconds(1);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HarvesterRuntime> _logger;
    private readonly AgentSettingsStore _store;
    private readonly HarvesterHealthState _health;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly object _gate = new();
    private Dictionary<FileSystemWatcher, WatcherBinding>? _maps;
    private readonly Dictionary<string, PendingFileEvent> _pendingFileEvents = new(StringComparer.OrdinalIgnoreCase);
    private CancellationTokenSource _watcherSetCancellation = new();
    private long _watcherSetGeneration;
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
    ///     Waits until a symbol file is completely written before it is eligible for upload. A file is
    ///     considered ready only when:
    ///     <list type="bullet">
    ///         <item>Its length and last-write-time remain unchanged over <see cref="StabilityRequiredConsecutiveSamples" />
    ///               consecutive probes (spaced by <see cref="StabilityProbeInterval" />).</item>
    ///         <item>It can be opened for read with the shared flags used by compilers and linkers.</item>
    ///         <item>Its header matches a known-good magic signature for the file format (see
    ///               <see cref="SymbolFileSignature" />). Guards against files whose length happens to
    ///               stabilize momentarily while still being initialized.</item>
    ///         <item>No other process still holds the file with write access, probed by attempting to
    ///               open the file without granting write-share. If the probe keeps failing after
    ///               <see cref="WriterLockProbeGracePeriod" />, the stability signal alone is accepted
    ///               so that files permanently opened by an IDE, AV scanner, or similar don't block
    ///               upload indefinitely.</item>
    ///     </list>
    ///     The combination tolerates slow network copies (SMB / NFS) where individual write bursts may
    ///     be tens to hundreds of milliseconds apart, while still rejecting truly in-progress writes
    ///     where data has not been fully flushed.
    ///     Returns the file version observed once the file is deemed ready. Throws a <see cref="TimeoutException" />
    ///     if the file does not stabilize within <see cref="FileReadyTimeout" />.
    /// </summary>
    private async Task<SymbolFileVersion> WaitForFileReadyAsync(string fullPath,
        CancellationToken cancellationToken)
    {
        DateTime deadline = DateTime.UtcNow + FileReadyTimeout;
        SymbolFileVersion lastObservation = default;
        int stableSamples = 0;
        DateTime? firstStableUtc = null;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException(
                    $"Symbol file {fullPath} did not reach a stable, readable state within {FileReadyTimeout.TotalSeconds:F0}s.");
            }

            SymbolFileVersion currentObservation;
            try
            {
                currentObservation = GetSymbolFileVersion(fullPath);
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
            catch (Exception)
            {
                await Task.Delay(StabilityProbeInterval, cancellationToken);
                continue;
            }

            // Zero-length means the writer only created the placeholder entry; keep waiting.
            if (currentObservation.Length == 0)
            {
                stableSamples = 0;
                lastObservation = currentObservation;
                firstStableUtc = null;
                await Task.Delay(StabilityProbeInterval, cancellationToken);
                continue;
            }

            if (lastObservation.Length == currentObservation.Length &&
                lastObservation.LastWriteTimeUtc == currentObservation.LastWriteTimeUtc)
            {
                stableSamples++;
            }
            else
            {
                stableSamples = 1;
                lastObservation = currentObservation;
                firstStableUtc = null;
            }

            if (stableSamples < StabilityRequiredConsecutiveSamples)
            {
                await Task.Delay(StabilityProbeInterval, cancellationToken);
                continue;
            }

            firstStableUtc ??= DateTime.UtcNow;

            // Length + timestamp stabilized across enough samples. Confirm we can actually read the file
            // and that the file contents look like a real symbol file (not just a sized placeholder).
            try
            {
                await using FileStream stream = new(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    SymbolFileShare,
                    bufferSize: 1024 * 8,
                    options: FileOptions.Asynchronous | FileOptions.SequentialScan);

                bool validMagic = await SymbolFileSignature.HasValidMagicAsync(stream, fullPath, cancellationToken);
                if (!validMagic)
                {
                    _logger.LogDebug(
                        "Symbol file {Path} length stabilized at {Length} bytes but its header does not match a known format; continuing to wait",
                        fullPath,
                        currentObservation.Length);

                    stableSamples = 0;
                    lastObservation = default;
                    firstStableUtc = null;
                    await Task.Delay(StabilityProbeInterval, cancellationToken);
                    continue;
                }
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
                // Read with shared-read access is blocked (rare, happens e.g. during SMB tail flush).
                // Don't reset firstStableUtc — this is still evidence a writer is active.
                await Task.Delay(StabilityProbeInterval, cancellationToken);
                continue;
            }

            // Writer-exclusion probe: try to open the file without granting write-share. On Windows and
            // SMB this fails with an IOException as long as any other handle was opened with write
            // access. Compilers/linkers and CopyFileEx/robocopy both open with write access while
            // writing; once they close the handle the probe succeeds. Gives a high-confidence "no one
            // is writing" signal that metadata alone cannot.
            bool writerStillPresent;
            try
            {
                await using FileStream probe = new(
                    fullPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read | FileShare.Delete,
                    bufferSize: 1,
                    options: FileOptions.Asynchronous);
                writerStillPresent = false;
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
                // Treat as "can't prove it's released"; rely on stability window grace period.
                writerStillPresent = true;
            }
            catch (IOException)
            {
                writerStillPresent = true;
            }

            if (writerStillPresent)
            {
                bool gracePeriodExpired = firstStableUtc.HasValue
                                          && DateTime.UtcNow - firstStableUtc.Value > WriterLockProbeGracePeriod;

                if (!gracePeriodExpired)
                {
                    await Task.Delay(StabilityProbeInterval, cancellationToken);
                    continue;
                }

                _logger.LogDebug(
                    "Writer still holds {Path} after {Elapsed:F1}s of stable metadata; accepting stability signal",
                    fullPath,
                    WriterLockProbeGracePeriod.TotalSeconds);
            }

            // Final version re-check so we can't accept a snapshot token for data that just changed.
            SymbolFileVersion afterRead;
            try
            {
                afterRead = GetSymbolFileVersion(fullPath);
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
            catch (Exception)
            {
                await Task.Delay(StabilityProbeInterval, cancellationToken);
                continue;
            }

            if (afterRead != currentObservation)
            {
                stableSamples = 1;
                lastObservation = afterRead;
                firstStableUtc = null;
                await Task.Delay(StabilityProbeInterval, cancellationToken);
                continue;
            }

            return currentObservation;
        }
    }

    /// <summary>
    ///     Signals that a source symbol file changed during snapshot capture and the snapshot must be discarded.
    /// </summary>
    private sealed class SnapshotVersionMismatchException(string message) : Exception(message);

    /// <summary>
    ///     Copies the symbol to a temp file using a short-lived shared read on the source, so uploads do not hold handles on build outputs.
    ///     Returns the temp path and the source file version that was snapshotted (length + last write UTC at copy start).
    ///     If the source file changes between the ready check and the end of the copy, a
    ///     <see cref="SnapshotVersionMismatchException" /> is thrown so the caller can discard the partial
    ///     snapshot and wait for a new stable state instead of uploading a torn read.
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
            throw new SnapshotVersionMismatchException(
                $"Symbol file {sourcePath} changed between ready check and snapshot start " +
                $"(wait token {versionAfterWait}, now {versionBeforeOpen}).");
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
            if (snapshottedVersion != versionAfterWait)
            {
                throw new SnapshotVersionMismatchException(
                    $"Symbol file {sourcePath} changed after opening for snapshot " +
                    $"(wait token {versionAfterWait}, now {snapshottedVersion}).");
            }

            await using FileStream dest = new(
                tempPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 1024 * 64,
                options: FileOptions.Asynchronous);

            await source.CopyToAsync(dest, cancellationToken);

            // Final guard: if the underlying file changed during the copy we may have captured a torn read.
            SymbolFileVersion versionAfterCopy = GetSymbolFileVersion(sourcePath);
            if (versionAfterCopy != versionAfterWait)
            {
                throw new SnapshotVersionMismatchException(
                    $"Symbol file {sourcePath} changed during snapshot copy " +
                    $"(wait token {versionAfterWait}, now {versionAfterCopy}).");
            }

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
    /// and servers for each watcher, attaches file event handlers, replaces any existing watchers atomically, updates
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
            InvalidatePendingFileEventsLocked();

            AgentSettingsDocument doc = _store.GetSnapshot();
            ServiceConfig serviceConfig = doc.ToServiceConfig();
            Dictionary<FileSystemWatcher, WatcherBinding> newMaps = new();
            Dictionary<FileSystemWatcher, WatcherBinding> oldMaps = _maps ?? new Dictionary<FileSystemWatcher, WatcherBinding>();

            try
            {
                // Group servers by watcher path, then create one watcher per unique path.
                Dictionary<(string Path, bool Recursive, string FiltersKey),
                    (HashSet<string> Filters, List<ServerConfig> Servers, List<WatcherPathEntry> WatchRules)> watcherGroups = new();

                foreach (ServerConfig serverConfig in serviceConfig.Servers)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (serverConfig.ServerUrl is null)
                    {
                        _logger.LogWarning("Skipping server entry with no ServerUrl");
                        continue;
                    }

                    foreach (WatcherPathEntry watch in serverConfig.WatcherPaths)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        string path = PathNormalization.NormalizeDirectoryPath(watch.Path);
                        if (string.IsNullOrEmpty(path))
                        {
                            continue;
                        }

                        IReadOnlyList<string> effectiveFilters = WatcherUploadFilterHelpers.GetEffectiveUploadFilters(watch);

                        bool recursive = watch.IncludeSubdirectories;
                        string filtersKey = WatcherUploadFilterHelpers.NormalizeFiltersKey(effectiveFilters);
                        (string Path, bool Recursive, string FiltersKey) key = (path, recursive, filtersKey);

                        if (!watcherGroups.TryGetValue(key, out var group))
                        {
                            group = (new HashSet<string>(StringComparer.OrdinalIgnoreCase), [], []);
                            watcherGroups[key] = group;
                        }

                        foreach (string pattern in effectiveFilters)
                        {
                            group.Filters.Add(pattern);
                        }

                        group.Servers.Add(serverConfig);
                        group.WatchRules.Add(watch);
                    }
                }

                foreach (KeyValuePair<(string Path, bool Recursive, string FiltersKey),
                             (HashSet<string> Filters, List<ServerConfig> Servers, List<WatcherPathEntry> WatchRules)> item in watcherGroups)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    FileSystemWatcher? watcher = null;
                    try
                    {
                        watcher = new FileSystemWatcher(item.Key.Path)
                        {
                            NotifyFilter = NotifyFilters.DirectoryName
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
                            Servers = item.Value.Servers
                                .Distinct()
                                .ToArray(),
                            WatchRules = item.Value.WatchRules.ToArray()
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
                    w.Created += WatcherOnFileEvent;
                    w.Changed += WatcherOnFileEvent;
                    w.Renamed += WatcherOnRenamed;
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
            InvalidatePendingFileEventsLocked();

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
            InvalidatePendingFileEventsLocked();
            DisposeWatchersLocked();
            _maps = null;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private void InvalidatePendingFileEventsLocked()
    {
        _watcherSetGeneration++;
        CancellationTokenSource oldCancellation = _watcherSetCancellation;
        oldCancellation.Cancel();
        oldCancellation.Dispose();
        _watcherSetCancellation = new CancellationTokenSource();
        _pendingFileEvents.Clear();
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
            map.Key.Created -= WatcherOnFileEvent;
            map.Key.Changed -= WatcherOnFileEvent;
            map.Key.Renamed -= WatcherOnRenamed;
            map.Key.EnableRaisingEvents = false;
            map.Key.Dispose();
        }

        maps.Clear();
        if (count > 0)
        {
            _logger.LogInformation("Watchers stopped");
        }
    }

    /// <summary>
    /// Begins background processing for a filesystem entry detected by a FileSystemWatcher.
    /// </summary>
    /// <remarks>
    /// Processing records the detection, waits for the file to become readable, snapshots the file to a temporary path,
    /// uploads the snapshot to the watcher’s configured servers, records per-server upload failures, and — if all uploads
    /// succeed and deletion rules match — attempts to delete the original file after verifying the file version unchanged.
    /// The temporary snapshot is removed when processing completes. Cancellation during application shutdown is honored
    /// and errors are logged and reported to runtime health.
    /// </remarks>
    /// <param name="sender">The <see cref="FileSystemWatcher"/> that raised the event.</param>
    /// <param name="e">The <see cref="FileSystemEventArgs"/> containing the detected file path.</param>
    private void WatcherOnFileEvent(object sender, FileSystemEventArgs e)
    {
        QueueDetectedFileEvent(sender, e);
    }

    private void WatcherOnRenamed(object sender, RenamedEventArgs e)
    {
        QueueDetectedFileEvent(sender, e);
    }

    private void QueueDetectedFileEvent(object sender, FileSystemEventArgs e)
    {
        FileSystemWatcher? watcher = sender as FileSystemWatcher;
        ArgumentNullException.ThrowIfNull(watcher);

        CancellationToken stopping = _lifetime.ApplicationStopping;

        PendingFileEvent? pendingToProcess = null;

        lock (_gate)
        {
            if (_maps is null || !_maps.TryGetValue(watcher, out WatcherBinding? binding))
            {
                return;
            }

            if (!watcher.EnableRaisingEvents)
            {
                return;
            }

            if (binding.Servers.Count == 0)
            {
                return;
            }

            string path = e.FullPath;
            string fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            if (_pendingFileEvents.TryGetValue(path, out PendingFileEvent? pending))
            {
                if (!pending.Bindings.Contains(binding))
                {
                    pending.Bindings.Add(binding);
                }

                pending.Revision++;
                _logger.LogTrace(
                    "Coalesced {ChangeType} event for {Path} into pending revision {Revision}",
                    e.ChangeType,
                    path,
                    pending.Revision);
                return;
            }

            pendingToProcess = new PendingFileEvent
            {
                Path = path,
                FileName = fileName,
                Bindings = [binding],
                WatcherSetGeneration = _watcherSetGeneration,
                WatcherSetCancellationToken = _watcherSetCancellation.Token,
                Revision = 1
            };
            _pendingFileEvents.Add(path, pendingToProcess);
        }

        if (pendingToProcess is null)
        {
            return;
        }

        PendingFileEvent queuedEvent = pendingToProcess;
        _ = Task.Run(
            () => ProcessPendingFileEventAsync(queuedEvent, stopping),
            stopping);
    }

    private async Task ProcessPendingFileEventAsync(PendingFileEvent pending, CancellationToken stopping)
    {
        using CancellationTokenSource linkedCancellation =
            CancellationTokenSource.CreateLinkedTokenSource(stopping, pending.WatcherSetCancellationToken);
        CancellationToken cancellationToken = linkedCancellation.Token;

        try
        {
            while (true)
            {
                int revision;
                WatcherBinding binding;

                lock (_gate)
                {
                    if (!_pendingFileEvents.TryGetValue(pending.Path, out PendingFileEvent? current) ||
                        !ReferenceEquals(current, pending) ||
                        pending.WatcherSetGeneration != _watcherSetGeneration)
                    {
                        return;
                    }

                    revision = pending.Revision;
                    binding = MergeBindings(pending.Bindings);
                }

                await Task.Delay(FileEventDebounceInterval, cancellationToken);

                lock (_gate)
                {
                    if (!_pendingFileEvents.TryGetValue(pending.Path, out PendingFileEvent? current) ||
                        !ReferenceEquals(current, pending) ||
                        pending.WatcherSetGeneration != _watcherSetGeneration)
                    {
                        return;
                    }

                    if (pending.Revision != revision)
                    {
                        continue;
                    }
                }

                try
                {
                    await ProcessDetectedFileAsync(binding, pending.Path, pending.FileName, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                {
                    _logger.LogDebug(ex, "Skipping symbol upload because {Path} no longer exists", pending.Path);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload symbol {FullPath}", pending.Path);
                    _health.RecordFileUploadFailure(pending.Path, null, null, ex.Message);
                    _health.RecordError(ex.Message);
                }

                lock (_gate)
                {
                    if (!_pendingFileEvents.TryGetValue(pending.Path, out PendingFileEvent? current) ||
                        !ReferenceEquals(current, pending) ||
                        pending.WatcherSetGeneration != _watcherSetGeneration)
                    {
                        return;
                    }

                    if (pending.Revision == revision)
                    {
                        _pendingFileEvents.Remove(pending.Path);
                        return;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // shutdown
        }
        finally
        {
            lock (_gate)
            {
                if (_pendingFileEvents.TryGetValue(pending.Path, out PendingFileEvent? current) &&
                    ReferenceEquals(current, pending))
                {
                    _pendingFileEvents.Remove(pending.Path);
                }
            }
        }
    }

    private static WatcherBinding MergeBindings(IReadOnlyList<WatcherBinding> bindings)
    {
        return new WatcherBinding
        {
            Servers = bindings
                .SelectMany(binding => binding.Servers)
                .Distinct()
                .ToArray(),
            WatchRules = bindings
                .SelectMany(binding => binding.WatchRules)
                .ToArray()
        };
    }

    private async Task ProcessDetectedFileAsync(WatcherBinding binding, string path, string fileName,
        CancellationToken stopping)
    {
        _health.RecordFileDetected(path);

        string? snapshotPath = null;
        SymbolFileVersion snapshottedVersion = default;
        SymbolFileVersion versionAfterWait = default;

        for (int attempt = 1; attempt <= SnapshotMaxAttempts; attempt++)
        {
            versionAfterWait = await WaitForFileReadyAsync(path, stopping);

            try
            {
                (snapshotPath, snapshottedVersion) =
                    await CreateUploadSnapshotAsync(path, versionAfterWait, stopping);
                break;
            }
            catch (SnapshotVersionMismatchException ex)
            {
                _logger.LogWarning(ex,
                    "Snapshot attempt {Attempt}/{Max} for {Path} detected a concurrent modification; retrying",
                    attempt,
                    SnapshotMaxAttempts,
                    path);

                if (attempt == SnapshotMaxAttempts)
                {
                    throw;
                }
            }
        }

        if (snapshotPath is null)
        {
            throw new InvalidOperationException(
                $"Could not capture a consistent snapshot of {path} after {SnapshotMaxAttempts} attempts.");
        }

        try
        {
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
            bool shouldDelete = ShouldDeleteAfterAllSuccess(binding.WatchRules, fileName);

            if (allSucceeded && shouldDelete)
            {
                try
                {
                    if (stopping.IsCancellationRequested)
                    {
                        _logger.LogWarning("Not deleting {Path}: cancellation requested before version probe", path);
                        _health.RecordFileDeleteSkipped(path, "Cancellation requested before deletion version probe");
                        return;
                    }

                    SymbolFileVersion currentVersion = GetSymbolFileVersion(path);
                    if (currentVersion != snapshottedVersion)
                    {
                        _logger.LogWarning(
                            "Not deleting {Path}: file changed after snapshot (snapshotted version {Snapshotted}, current {Current})",
                            path,
                            snapshottedVersion,
                            currentVersion);
                        _health.RecordFileDeleteSkipped(
                            path,
                            $"Version mismatch: snapshotted {snapshottedVersion}, current {currentVersion}");
                    }
                    else
                    {
                        if (stopping.IsCancellationRequested)
                        {
                            _logger.LogWarning("Not deleting {Path}: cancellation requested before delete", path);
                            _health.RecordFileDeleteSkipped(path, "Cancellation requested before delete");
                            return;
                        }

                        File.Delete(path);
                        _logger.LogInformation("Symbol file {Symbol} deleted", path);
                        _health.RecordFileDeleted(path);
                    }
                }
                catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
                {
                    _logger.LogWarning("Not deleting {Path}: file no longer exists or directory vanished", path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not verify or delete {Path} after upload", path);
                    _health.RecordFileDeleteFailed(path, ex.Message);
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

    /// <summary>
    /// Uploads a snapshotted symbol file to the specified server and reports the outcome.
    /// </summary>
    /// <param name="serverConfig">Destination server configuration.</param>
    /// <param name="snapshotPath">File system path to the temporary snapshot of the symbol file to upload.</param>
    /// <param name="detectedPath">Original detected file path (used for health reporting and writing any upload-error file).</param>
    /// <param name="fileName">File name to include in the multipart upload.</param>
    /// <param name="mimeType">MIME type to set for the uploaded file content.</param>
    /// <param name="snapshottedSourceVersion">Captured source file version at snapshot time (used for logging and diagnostics).</param>
    /// <param name="cancellationToken">Token to cancel the upload operation.</param>
    /// <returns>
    /// An <see cref="UploadAttemptResult"/> with:
    /// - <see cref="UploadAttemptResult.Success"/>: whether the server accepted the upload.
    /// - <see cref="UploadAttemptResult.ServerDisplayName"/> and <see cref="UploadAttemptResult.ServerUrl"/>: target server identity.
    /// - <see cref="UploadAttemptResult.ErrorDetails"/>: populated when the upload failed.
    /// Whether to delete the original file after uploads is not part of <see cref="UploadAttemptResult"/>; it is decided at the watcher level
    /// (see <see cref="ShouldDeleteAfterAllSuccess(IReadOnlyList{WatcherPathEntry}, string)"/>) after all per-server uploads for the event complete.
    /// </returns>
    /// <exception cref="OperationCanceledException">If the operation is canceled via the <paramref name="cancellationToken"/>.</exception>
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

                return new UploadAttemptResult
                {
                    Success = true,
                    ServerDisplayName = serverDisplayName,
                    ServerUrl = serverUrl
                };
            }

            _logger.LogInformation("Symbol upload failed");
            string body = await response.Content.ReadAsStringAsync(cancellationToken);
            _health.RecordUploadFailure($"HTTP {(int)response.StatusCode}: {body}");
            await WriteUploadErrorSidecarAsync(detectedPath, body, cancellationToken);
            return new UploadAttemptResult
            {
                Success = false,
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
                ServerDisplayName = serverDisplayName,
                ServerUrl = serverUrl,
                ErrorDetails = ex.Message
            };
        }
    }

    /// <summary>
    /// Writes <c>{path}.upload-error.txt</c> next to the symbol file. Retries on typical sharing violations (AV, IDE,
    /// concurrent writers) and falls back to a timestamped file name if the primary sidecar stays locked.
    /// </summary>
    private async Task WriteUploadErrorSidecarAsync(string detectedPath, string body,
        CancellationToken cancellationToken)
    {
        string primary = $"{detectedPath}.upload-error.txt";

        for (int attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                await File.WriteAllTextAsync(primary, body, cancellationToken);
                return;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                if (attempt == 2)
                {
                    _logger.LogWarning(ex, "Could not write upload error sidecar {Path} after retries", primary);
                    break;
                }

                await Task.Delay(50 * (1 << attempt), cancellationToken);
            }
        }

        string fallback = $"{detectedPath}.upload-error.{DateTime.UtcNow:yyyyMMddHHmmssfff}.txt";

        try
        {
            await File.WriteAllTextAsync(fallback, body, cancellationToken);
            _logger.LogInformation("Wrote upload error details to fallback path {Path}", fallback);
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
                throw;

            for (Exception? inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                if (inner is OperationCanceledException)
                    throw inner;
            }

            _logger.LogWarning(ex, "Could not write upload error sidecar at {Path} or {Fallback}", primary, fallback);
        }
    }

    private static readonly string[] DefaultDeletionInclusionPatterns = ["**/*.pdb"];

    private static bool ShouldDeleteAfterAllSuccess(IReadOnlyList<WatcherPathEntry> watchRules, string fileName)
    {
        foreach (WatcherPathEntry rule in watchRules)
        {
            if (!rule.DeleteAfterUpload)
            {
                continue;
            }

            Matcher matcher = new();
            IReadOnlyList<string> includes = rule.DeletionInclusionFilter is { Count: > 0 }
                ? rule.DeletionInclusionFilter
                : DefaultDeletionInclusionPatterns;
            matcher.AddIncludePatterns(includes);
            matcher.AddExcludePatterns(rule.DeletionExclusionFilter ?? []);

            PatternMatchingResult match = matcher.Match(fileName);
            if (match.HasMatches)
            {
                return true;
            }
        }

        return false;
    }
}
