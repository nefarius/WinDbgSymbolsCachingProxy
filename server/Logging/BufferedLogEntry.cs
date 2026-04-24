using Serilog.Events;

namespace WinDbgSymbolsCachingProxy.Logging;

public sealed record BufferedLogEntry(
    DateTimeOffset Timestamp,
    LogEventLevel Level,
    string Message,
    string? Exception,
    string? SourceContext);
