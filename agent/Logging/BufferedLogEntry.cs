using Serilog.Events;

namespace HarvestingAgent.Logging;

public sealed record BufferedLogEntry(
    DateTimeOffset Timestamp,
    LogEventLevel Level,
    string Message,
    string? Exception,
    string? SourceContext);
