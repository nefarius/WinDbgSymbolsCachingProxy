using System.Linq;

namespace HarvestingAgent;

public sealed class WatcherStatusEntry
{
    public required string Path { get; init; }

    public bool Recursive { get; init; }

    public bool EventsEnabled { get; init; }
}

public enum FileActivityStatus
{
    Detected,
    UploadSucceeded,
    UploadFailed,
    Deleted,
    DeleteFailed
}

public sealed class HarvestedFileHistoryEntry
{
    public required DateTimeOffset TimestampUtc { get; init; }

    public required string FilePath { get; init; }

    public required string FileName { get; init; }

    public required FileActivityStatus Status { get; init; }

    public string? ServerUrl { get; init; }

    public string? ServerDisplayName { get; init; }

    public string? Details { get; init; }
}

public sealed class HarvesterHealthState
{
    private const int MaxHistoryEntries = 300;
    private readonly object _lock = new();

    public event Action? Changed;

    public bool HarvestingEnabled { get; private set; }

    public int ActiveWatcherCount { get; private set; }

    public IReadOnlyList<WatcherStatusEntry> Watchers { get; private set; } = [];

    public IReadOnlyList<HarvestedFileHistoryEntry> FileActivityHistory { get; private set; } = [];

    public long UploadSuccessCount { get; private set; }

    public long UploadFailureCount { get; private set; }

    public DateTimeOffset? LastUploadUtc { get; private set; }

    public DateTimeOffset? LastErrorUtc { get; private set; }

    public string? LastErrorMessage { get; private set; }

    public void SetRuntimeState(bool harvestingEnabled, IReadOnlyList<WatcherStatusEntry> watchers)
    {
        lock (_lock)
        {
            HarvestingEnabled = harvestingEnabled;
            Watchers = watchers;
            ActiveWatcherCount = watchers.Count(w => w.EventsEnabled);
        }

        RaiseChanged();
    }

    public void RecordUploadSuccess()
    {
        lock (_lock)
        {
            UploadSuccessCount++;
            LastUploadUtc = DateTimeOffset.UtcNow;
        }

        RaiseChanged();
    }

    public void RecordUploadFailure(string message)
    {
        lock (_lock)
        {
            UploadFailureCount++;
            LastErrorUtc = DateTimeOffset.UtcNow;
            LastErrorMessage = message;
        }

        RaiseChanged();
    }

    public void RecordError(string message)
    {
        lock (_lock)
        {
            LastErrorUtc = DateTimeOffset.UtcNow;
            LastErrorMessage = message;
        }

        RaiseChanged();
    }

    /// <summary>
    /// Records that a file was detected and adds an entry to the file activity history.
    /// </summary>
    /// <param name="filePath">The full path of the detected file.</param>
    /// <remarks>
    /// The recorded history entry uses the current UTC timestamp, derives the file name from <paramref name="filePath"/>, and marks the status as detected. After updating history, the <c>Changed</c> event is raised.
    /// </remarks>
    public void RecordFileDetected(string filePath)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.Detected
            });
        }

        RaiseChanged();
    }

    /// <summary>
    /// Record a successful upload for the specified file and add a corresponding entry to the file activity history.
    /// </summary>
    /// <param name="filePath">The full path of the uploaded file; the entry's FileName is derived from this path.</param>
    /// <param name="serverDisplayName">An optional human-friendly name for the upload destination.</param>
    /// <param name="serverUrl">An optional URL of the upload destination.</param>
    /// <remarks>
    /// The history entry's timestamp is set to the current UTC time. This method updates internal counters/state and raises the <c>Changed</c> event after the update.
    /// </remarks>
    public void RecordFileUploadSuccess(string filePath, string? serverDisplayName, string? serverUrl)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.UploadSucceeded,
                ServerDisplayName = serverDisplayName,
                ServerUrl = serverUrl
            });
        }

        RaiseChanged();
    }

    /// <summary>
    /// Record a failed upload for a harvested file and add a corresponding entry to the in-memory history.
    /// </summary>
    /// <param name="filePath">Full path of the file that failed to upload.</param>
    /// <param name="serverDisplayName">Optional display name of the server/endpoint that was the upload target.</param>
    /// <param name="serverUrl">Optional URL of the server/endpoint that was the upload target.</param>
    /// <param name="details">Error or diagnostic details describing the failure.</param>
    /// <remarks>
    /// The method creates a history entry with the current UTC timestamp, derives <c>FileName</c> from <c>filePath</c>, and marks the entry with <see cref="FileActivityStatus.UploadFailed"/>. State mutation is performed under an internal lock and the <see cref="Changed"/> event is raised after the update. The new entry is inserted as the most recent history item and the history is trimmed to the configured maximum length.
    /// </remarks>
    public void RecordFileUploadFailure(string filePath, string? serverDisplayName, string? serverUrl, string details)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.UploadFailed,
                ServerDisplayName = serverDisplayName,
                ServerUrl = serverUrl,
                Details = details
            });
        }

        RaiseChanged();
    }

    /// <summary>
    ///     Records that the local file was removed after a successful upload (delete-after-upload).
    /// </summary>
    public void RecordFileDeleted(string filePath)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.Deleted
            });
        }

        RaiseChanged();
    }

    /// <summary>
    ///     Records that delete-after-upload failed for the given path.
    /// </summary>
    public void RecordFileDeleteFailed(string filePath, string details)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.DeleteFailed,
                Details = details
            });
        }

        RaiseChanged();
    }

    /// <summary>
    ///     Removes all entries from the recent file activity history. Does not reset upload counters or last-error fields.
    /// </summary>
    public void ClearFileActivityHistory()
    {
        lock (_lock)
        {
            FileActivityHistory = Array.Empty<HarvestedFileHistoryEntry>();
        }

        RaiseChanged();
    }

    private void AddHistoryEntry(HarvestedFileHistoryEntry entry)
    {
        List<HarvestedFileHistoryEntry> list = [.. FileActivityHistory];
        list.Insert(0, entry);
        if (list.Count > MaxHistoryEntries)
        {
            list.RemoveRange(MaxHistoryEntries, list.Count - MaxHistoryEntries);
        }

        FileActivityHistory = list.AsReadOnly();
    }

    private void RaiseChanged()
    {
        Changed?.Invoke();
    }
}
