using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.FileSystemGlobbing;

using MimeTypes;

using Polly.CircuitBreaker;

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

        /// <summary>Set when the server's circuit breaker is open; the upload should be retried after this delay.</summary>
        public TimeSpan? RetryAfter { get; init; }
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
    ///     Hard deadline for one upload including response-body buffering, which the resilience pipeline's total timeout
    ///     (45 min) does not cover.
    /// </summary>
    private static readonly TimeSpan UploadRequestDeadline = TimeSpan.FromMinutes(50);

    /// <summary>
    ///     How often a file event is re-queued because a server circuit was open before giving up.
    /// </summary>
    private const int MaxCircuitDeferrals = 10;

    private static readonly TimeSpan DefaultCircuitRetryDelay = TimeSpan.FromSeconds(35);

    /// <summary>
    ///     Coalesces the burst of change notifications emitted while compilers and linkers are writing build outputs.
    /// </summary>
    private static readonly TimeSpan FileEventDebounceInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    ///     Dedicated folder for upload snapshots so leftovers from crashes can be identified and removed safely.
    ///     Lives under the agent's protected data directory (not the shared temp folder) because snapshots are copies of
    ///     files that may be sensitive; access is restricted before first use.
    /// </summary>
    private static readonly string SnapshotDirectory =
        Path.Combine(AgentSettingsPaths.SettingsDirectory, "snapshots");

    /// <summary>Windows and macOS volumes are case-insensitive by default; Linux ones are not.</summary>
    private static StringComparer PathComparer =>
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static bool IsInSnapshotDirectory(string path)
    {
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(SnapshotDirectory));
        string full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));

        // The directory itself or anything beneath it (separator-aware, so "snapshots2" does not match).
        return string.Equals(full, root, PathComparison) ||
               full.StartsWith(root + Path.DirectorySeparatorChar, PathComparison);
    }

    private static void EnsureSnapshotDirectory()
    {
        Directory.CreateDirectory(SnapshotDirectory);
        AgentSettingsPaths.RestrictSettingsDirectory(SnapshotDirectory);
    }

    /// <summary>
    ///     Maximum number of files being waited on, snapshotted and uploaded at the same time.
    /// </summary>
    private const int MaxConcurrentFileProcessing = 4;

    /// <summary>
    ///     Large buffer so bursts of build output do not overflow the watcher (default is 8 KB; the maximum is 64 KB).
    /// </summary>
    private const int WatcherInternalBufferSize = 64 * 1024;

    private readonly SemaphoreSlim _processingSlots = new(MaxConcurrentFileProcessing, MaxConcurrentFileProcessing);

    /// <summary>
    ///     Last file version that was uploaded successfully to all servers, per path; identical re-notifications are skipped.
    /// </summary>
    private readonly Dictionary<string, UploadedRecord> _uploadedVersions = new(PathComparer);

    /// <summary>File version of a path plus the servers (by URL) that already received exactly that version.</summary>
    private sealed record UploadedRecord(SymbolFileVersion Version, HashSet<string> Servers);

    private static string ServerKey(ServerConfig server) => server.ServerUrl?.ToString() ?? string.Empty;

    /// <summary>Held while snapshotting/uploading/deleting; deliberately not held while waiting for file readiness.</summary>
    private sealed class ProcessingSlot(SemaphoreSlim semaphore)
    {
        private bool _held;

        public async Task AcquireAsync(CancellationToken cancellationToken)
        {
            if (_held)
            {
                return;
            }

            await semaphore.WaitAsync(cancellationToken);
            _held = true;
        }

        public void Release()
        {
            if (_held)
            {
                _held = false;
                semaphore.Release();
            }
        }
    }

    private int _unavailablePaths;
    private int _recoveryRunning;
    private bool _enabled;

    /// <summary>Everything needed to re-create a single watcher without touching the healthy ones.</summary>
    private sealed record WatcherSpec(string Path, bool Recursive, string[] Filters, WatcherBinding Binding);

    private readonly List<WatcherSpec> _failedWatchers = [];

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HarvesterRuntime> _logger;
    private readonly AgentSettingsStore _store;
    private readonly HarvesterHealthState _health;
    private readonly IHostApplicationLifetime _lifetime;

    private readonly object _gate = new();
    private Dictionary<FileSystemWatcher, WatcherBinding>? _maps;
    private readonly Dictionary<string, PendingFileEvent> _pendingFileEvents = new(PathComparer);
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
        CleanupStaleSnapshots();
    }

    /// <summary>
    ///     Removes snapshot files left behind by a previous process that crashed or was killed mid-upload.
    /// </summary>
    private void CleanupStaleSnapshots()
    {
        try
        {
            if (Directory.Exists(SnapshotDirectory))
            {
                EnsureSnapshotDirectory();
            }

            if (!Directory.Exists(SnapshotDirectory))
            {
                return;
            }

            foreach (string file in Directory.EnumerateFiles(SnapshotDirectory, "harvester-*"))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    _logger.LogDebug(ex, "Could not delete stale snapshot {Path}", file);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogDebug(ex, "Could not enumerate stale snapshots in {Directory}", SnapshotDirectory);
        }
    }

    private static bool IsUploadErrorSidecar(string fileName)
    {
        return fileName.Contains(".upload-error", StringComparison.OrdinalIgnoreCase) &&
               fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
    }

    private void WatcherOnError(object sender, ErrorEventArgs e)
    {
        Exception ex = e.GetException();
        _logger.LogError(ex, "File system watcher error (events may have been lost); re-creating only the failed watcher");
        _health.RecordError($"Watcher error: {ex.Message}");

        // Only the failed watcher is replaced; pending events and in-flight uploads of healthy watchers are untouched.
        lock (_gate)
        {
            if (_maps is not null && sender is FileSystemWatcher failed &&
                _maps.TryGetValue(failed, out WatcherBinding? binding))
            {
                _failedWatchers.Add(new WatcherSpec(
                    failed.Path, failed.IncludeSubdirectories, failed.Filters.ToArray(), binding));
                _maps.Remove(failed);
                DetachAndDispose(failed);
                PublishStatusLocked();
                Volatile.Write(ref _unavailablePaths, _failedWatchers.Count);
            }
        }

        StartWatcherRecovery();
    }

    private void DetachAndDispose(FileSystemWatcher watcher)
    {
        watcher.Created -= WatcherOnFileEvent;
        watcher.Changed -= WatcherOnFileEvent;
        watcher.Renamed -= WatcherOnRenamed;
        watcher.Error -= WatcherOnError;
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
    }

    private void PublishStatusLocked()
    {
        List<WatcherStatusEntry> status = [];
        if (_maps is not null)
        {
            foreach (FileSystemWatcher w in _maps.Keys)
            {
                status.Add(new WatcherStatusEntry
                {
                    Path = w.Path,
                    Recursive = w.IncludeSubdirectories,
                    EventsEnabled = w.EnableRaisingEvents
                });
            }
        }

        _health.SetRuntimeState(_enabled, status.AsReadOnly());
    }

    /// <summary>
    ///     Re-creates failed watchers individually, with growing delays, until every one is back (e.g. a network share
    ///     that went away returned). Only one recovery loop runs at a time. Files that appeared while a watcher was
    ///     down are not replayed.
    /// </summary>
    private void StartWatcherRecovery()
    {
        if (Volatile.Read(ref _unavailablePaths) == 0 || _lifetime.ApplicationStopping.IsCancellationRequested ||
            _disposed)
        {
            return;
        }

        if (Interlocked.Exchange(ref _recoveryRunning, 1) == 1)
        {
            return;
        }

        CancellationToken stopping = _lifetime.ApplicationStopping;
        _ = Task.Run(async () =>
        {
            TimeSpan delay = TimeSpan.FromSeconds(5);
            try
            {
                while (!stopping.IsCancellationRequested)
                {
                    await Task.Delay(delay, stopping);

                    if (!TryRestoreFailedWatchers())
                    {
                        return;
                    }

                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 60));
                }
            }
            catch (OperationCanceledException)
            {
                // shutting down
            }
            finally
            {
                Volatile.Write(ref _recoveryRunning, 0);
            }

            // A watcher may have failed between the last check and resetting the flag.
            StartWatcherRecovery();
        });
    }

    /// <returns>True while watchers are still failed and another attempt is needed.</returns>
    private bool TryRestoreFailedWatchers()
    {
        lock (_gate)
        {
            if (_disposed || _maps is null)
            {
                _failedWatchers.Clear();
                Volatile.Write(ref _unavailablePaths, 0);
                return false;
            }

            foreach (WatcherSpec spec in _failedWatchers.ToList())
            {
                FileSystemWatcher? watcher = null;
                try
                {
                    watcher = new FileSystemWatcher(spec.Path)
                    {
                        NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName |
                                       NotifyFilters.LastWrite | NotifyFilters.Size,
                        IncludeSubdirectories = spec.Recursive,
                        InternalBufferSize = WatcherInternalBufferSize
                    };

                    foreach (string pattern in spec.Filters)
                    {
                        watcher.Filters.Add(pattern);
                    }

                    watcher.Created += WatcherOnFileEvent;
                    watcher.Changed += WatcherOnFileEvent;
                    watcher.Renamed += WatcherOnRenamed;
                    watcher.Error += WatcherOnError;
                    watcher.EnableRaisingEvents = _enabled;

                    _maps.Add(watcher, spec.Binding);
                    _failedWatchers.Remove(spec);
                    _logger.LogInformation("Watcher for {Path} restored", spec.Path);
                    watcher = null;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Watcher for {Path} is still unavailable", spec.Path);
                    watcher?.Dispose();
                }
            }

            PublishStatusLocked();
            Volatile.Write(ref _unavailablePaths, _failedWatchers.Count);
            return _failedWatchers.Count > 0;
        }
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
        EnsureSnapshotDirectory();
        string tempName = $"harvester-{Guid.NewGuid():N}-{Path.GetFileName(sourcePath)}";
        string tempPath = Path.Combine(SnapshotDirectory, tempName);

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
            // Configuration may have added servers; previously uploaded files must be eligible again.
            _uploadedVersions.Clear();
            _failedWatchers.Clear();

            AgentSettingsDocument doc = _store.GetSnapshot();
            ServiceConfig serviceConfig = doc.ToServiceConfig();
            Dictionary<FileSystemWatcher, WatcherBinding> newMaps = new();
            int unavailable = 0;
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
                            IncludeSubdirectories = item.Key.Recursive,
                            InternalBufferSize = WatcherInternalBufferSize
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
                        unavailable++;
                        _failedWatchers.Add(new WatcherSpec(
                            item.Key.Path,
                            item.Key.Recursive,
                            item.Value.Filters.ToArray(),
                            new WatcherBinding
                            {
                                Servers = item.Value.Servers.Distinct().ToArray(),
                                WatchRules = item.Value.WatchRules.ToArray()
                            }));
                        watcher?.Dispose();
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();

                bool enable = doc.HarvestingEnabled;
                _enabled = enable;
                List<WatcherStatusEntry> status = [];

                foreach (KeyValuePair<FileSystemWatcher, WatcherBinding> map in newMaps)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    FileSystemWatcher w = map.Key;
                    w.Created += WatcherOnFileEvent;
                    w.Changed += WatcherOnFileEvent;
                    w.Renamed += WatcherOnRenamed;
                    w.Error += WatcherOnError;
                    w.EnableRaisingEvents = enable;
                    status.Add(new WatcherStatusEntry
                    {
                        Path = w.Path,
                        Recursive = w.IncludeSubdirectories,
                        EventsEnabled = w.EnableRaisingEvents
                    });
                }

                // Publish only after the new collection is fully initialized.
                _unavailablePaths = unavailable;
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

        // Paths that are unavailable now (offline share, missing folder) are retried in the background.
        StartWatcherRecovery();
    }

    public Task SetHarvestingEnabledAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            _store.UpdateHarvestingEnabled(enabled);
            _enabled = enabled;
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
            _failedWatchers.Clear();
            Volatile.Write(ref _unavailablePaths, 0);
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
            map.Key.Error -= WatcherOnError;
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
            if (IsInSnapshotDirectory(path))
            {
                // Our own upload snapshots must never be harvested (a watched parent folder would loop).
                return;
            }

            string fileName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(fileName) || IsUploadErrorSidecar(fileName))
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

                    binding = MergeBindings(pending.Bindings);
                }

                try
                {
                    await ProcessDetectedFileAsync(binding, pending.Path, pending.FileName, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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
        ProcessingSlot slot = new(_processingSlots);
        try
        {
            await ProcessDetectedFileCoreAsync(binding, path, fileName, slot, stopping);
        }
        finally
        {
            slot.Release();
        }
    }

    private async Task ProcessDetectedFileCoreAsync(WatcherBinding binding, string path, string fileName,
        ProcessingSlot slot, CancellationToken stopping)
    {
        _health.RecordFileDetected(path);

        string? snapshotPath = null;
        List<ServerConfig> targets = binding.Servers.ToList();
        SymbolFileVersion snapshottedVersion = default;
        SymbolFileVersion versionAfterWait = default;

        for (int attempt = 1; attempt <= SnapshotMaxAttempts; attempt++)
        {
            versionAfterWait = await WaitForFileReadyAsync(path, stopping);

            // Only servers that have not yet received exactly this version need an upload.
            lock (_gate)
            {
                targets = _uploadedVersions.TryGetValue(path, out UploadedRecord? uploaded) &&
                          uploaded.Version == versionAfterWait
                    ? binding.Servers.Where(s => !uploaded.Servers.Contains(ServerKey(s))).ToList()
                    : binding.Servers.ToList();
            }

            if (targets.Count == 0)
            {
                // Nothing to upload, but the deletion step below must still run (it may have failed earlier).
                _logger.LogDebug("{Path}: this version was already uploaded to all servers", path);
                snapshottedVersion = versionAfterWait;
                break;
            }

            // The wait for readiness above deliberately happens without a slot.
            await slot.AcquireAsync(stopping);

            try
            {
                (snapshotPath, snapshottedVersion) =
                    await CreateUploadSnapshotAsync(path, versionAfterWait, stopping);
                break;
            }
            catch (SnapshotVersionMismatchException ex)
            {
                slot.Release();
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

        if (snapshotPath is null && targets.Count > 0)
        {
            throw new InvalidOperationException(
                $"Could not capture a consistent snapshot of {path} after {SnapshotMaxAttempts} attempts.");
        }

        try
        {
            string extension = Path.GetExtension(path);
            string? mimeType = MimeTypeMap.GetMimeType(extension);
            if (string.IsNullOrEmpty(mimeType))
            {
                mimeType = "application/octet-stream";
                _logger.LogWarning(
                    "Could not resolve MIME type for {Path} with extension {Extension}; using {MimeType}",
                    path,
                    extension,
                    mimeType);
            }

            // Destinations rejected by an open circuit are retried here, reusing the snapshot and never resending to
            // destinations that already succeeded or failed for another reason.
            List<UploadAttemptResult> finalResults = [];
            List<ServerConfig> remaining = targets.ToList();

            for (int round = 1; ; round++)
            {
                ServerConfig[] roundServers = remaining.ToArray();
                UploadAttemptResult[] roundResults = await Task.WhenAll(roundServers.Select(serverConfig =>
                    UploadToServerAsync(
                        serverConfig,
                        snapshotPath!,
                        path,
                        fileName,
                        mimeType,
                        snapshottedVersion,
                        stopping)));

                List<ServerConfig> deferred = [];
                TimeSpan delay = TimeSpan.Zero;
                for (int i = 0; i < roundResults.Length; i++)
                {
                    UploadAttemptResult result = roundResults[i];
                    if (result.RetryAfter is { } retryAfter && round < MaxCircuitDeferrals)
                    {
                        deferred.Add(roundServers[i]);
                        delay = retryAfter > delay ? retryAfter : delay;
                        continue;
                    }

                    finalResults.Add(result);
                    if (!result.Success)
                    {
                        _health.RecordFileUploadFailure(
                            path,
                            result.ServerDisplayName,
                            result.ServerUrl,
                            result.ErrorDetails ?? "Upload failed");
                    }
                }

                if (deferred.Count == 0)
                {
                    break;
                }

                // Each file revision (one call) has its own retry allowance of MaxCircuitDeferrals rounds.
                _logger.LogWarning(
                    "{Count} server circuit(s) open; retrying {Path} in {Delay:F0}s (round {Round}/{Max})",
                    deferred.Count, path, delay.TotalSeconds, round, MaxCircuitDeferrals);
                // The slot stays held: the snapshot is kept across retries, so live snapshots remain bounded by the slot limit.
                await Task.Delay(delay, stopping);
                remaining = deferred;
            }

            UploadAttemptResult[] results = finalResults.ToArray();
            bool allSucceeded = results.All(r => r.Success);

            // Remember per server what was delivered so a partial failure only retries the missing destinations.
            lock (_gate)
            {
                if (_uploadedVersions.Count > 10_000)
                {
                    _uploadedVersions.Clear();
                }

                if (!_uploadedVersions.TryGetValue(path, out UploadedRecord? record) ||
                    record.Version != snapshottedVersion)
                {
                    record = new UploadedRecord(snapshottedVersion, new HashSet<string>(StringComparer.Ordinal));
                    _uploadedVersions[path] = record;
                }

                foreach (UploadAttemptResult result in results.Where(r => r.Success && r.ServerUrl is not null))
                {
                    record.Servers.Add(result.ServerUrl!);
                }
            }

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
            // A trailing slash keeps the relative upload path below the configured base path (reverse proxies / sub-paths).
            string baseUrl = serverConfig.ServerUrl.ToString();
            client.BaseAddress = baseUrl.EndsWith('/') ? serverConfig.ServerUrl : new Uri(baseUrl + "/", UriKind.Absolute);

            if (!string.IsNullOrWhiteSpace(serverConfig.ApiKey))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Api-Key", serverConfig.ApiKey);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            $"{serverConfig.Authentication.Username}:{serverConfig.Authentication.Password}")));
            }

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

            // Finite deadline for the whole exchange, including buffering of the response body.
            using CancellationTokenSource requestCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            requestCts.CancelAfter(UploadRequestDeadline);

            using HttpResponseMessage response = await client.PostAsync(
                "api/uploads/symbol?force=true",
                form,
                requestCts.Token);

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
            string body = await response.Content.ReadAsStringAsync(requestCts.Token);
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (BrokenCircuitException ex)
        {
            _logger.LogWarning("Upload to {ServerUrl} rejected: circuit is open", serverUrl);
            _health.RecordUploadFailure("Circuit open: " + ex.Message);
            return new UploadAttemptResult
            {
                Success = false,
                ServerDisplayName = serverDisplayName,
                ServerUrl = serverUrl,
                ErrorDetails = "Circuit open: " + ex.Message,
                RetryAfter = DefaultCircuitRetryDelay
            };
        }
        catch (Exception ex)
        {
            // Includes the upload deadline expiring (cancellation not requested by the caller), which counts as a failure.
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
