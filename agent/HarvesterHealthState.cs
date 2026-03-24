namespace HarvestingAgent;

public sealed class WatcherStatusEntry
{
    public required string Path { get; init; }

    public bool Recursive { get; init; }

    public bool EventsEnabled { get; init; }
}

public sealed class HarvesterHealthState
{
    private readonly object _lock = new();

    public event Action? Changed;

    public bool HarvestingEnabled { get; private set; }

    public int ActiveWatcherCount { get; private set; }

    public IReadOnlyList<WatcherStatusEntry> Watchers { get; private set; } = [];

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
            ActiveWatcherCount = watchers.Count;
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

    private void RaiseChanged()
    {
        Changed?.Invoke();
    }
}
