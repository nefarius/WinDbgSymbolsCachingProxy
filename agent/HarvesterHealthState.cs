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
    UploadFailed
}

public sealed class HarvestedFileHistoryEntry
{
    public required DateTimeOffset TimestampUtc { get; init; }

    public required string FilePath { get; init; }

    public required string FileName { get; init; }

    public required FileActivityStatus Status { get; init; }

    public string? ServerUrl { get; init; }

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

    public void RecordFileUploadSuccess(string filePath, string? serverUrl)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.UploadSucceeded,
                ServerUrl = serverUrl
            });
        }

        RaiseChanged();
    }

    public void RecordFileUploadFailure(string filePath, string? serverUrl, string details)
    {
        lock (_lock)
        {
            AddHistoryEntry(new HarvestedFileHistoryEntry
            {
                TimestampUtc = DateTimeOffset.UtcNow,
                FilePath = filePath,
                FileName = Path.GetFileName(filePath),
                Status = FileActivityStatus.UploadFailed,
                ServerUrl = serverUrl,
                Details = details
            });
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
