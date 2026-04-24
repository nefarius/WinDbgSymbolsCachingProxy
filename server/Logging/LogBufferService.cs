using Serilog.Events;

namespace WinDbgSymbolsCachingProxy.Logging;

public sealed class LogBufferService
{
    public const int Capacity = 200;

    private readonly Lock _gate = new();
    private readonly Queue<BufferedLogEntry> _entries = new(Capacity);

    public event Action? Changed;

    public IReadOnlyList<BufferedLogEntry> GetSnapshot()
    {
        lock (_gate)
        {
            return _entries
                .Reverse()
                .ToArray();
        }
    }

    public void Append(LogEvent logEvent)
    {
        string? sourceContext = null;

        if (logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContextValue) &&
            sourceContextValue is ScalarValue { Value: string value })
        {
            sourceContext = value;
        }

        BufferedLogEntry entry = new(
            logEvent.Timestamp,
            logEvent.Level,
            logEvent.RenderMessage(),
            logEvent.Exception?.ToString(),
            sourceContext);

        lock (_gate)
        {
            if (_entries.Count == Capacity)
            {
                _entries.Dequeue();
            }

            _entries.Enqueue(entry);
        }

        Changed?.Invoke();
    }

    public void Clear()
    {
        bool hadEntries;

        lock (_gate)
        {
            hadEntries = _entries.Count > 0;
            _entries.Clear();
        }

        if (hadEntries)
        {
            Changed?.Invoke();
        }
    }
}
