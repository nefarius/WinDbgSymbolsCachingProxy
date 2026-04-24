using Serilog.Core;
using Serilog.Events;

namespace HarvestingAgent.Logging;

public sealed class LogBufferSink(LogBufferService buffer) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        buffer.Append(logEvent);
    }
}
