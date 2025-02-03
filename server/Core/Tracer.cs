using Microsoft.SymbolStore;

using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Simple trace/logging support. Used by <see cref="SymStoreService"/>.
/// </summary>
/// <remarks>TODO: pipe this into OTEL in the future.</remarks>
internal sealed class Tracer : ITracer
{
    public bool Enabled { get; set; }
    public bool EnabledVerbose { get; set; }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteLine(string format, params object[] arguments)
    {
        Console.WriteLine(format, arguments);
    }

    public void Information(string message)
    {
        if (Enabled)
        {
            Console.WriteLine(message);
        }
    }

    public void Information(string format, params object[] arguments)
    {
        if (Enabled)
        {
            Console.WriteLine(format, arguments);
        }
    }

    public void Warning(string message)
    {
        if (Enabled)
        {
            Console.WriteLine("WARNING: " + message);
        }
    }

    public void Warning(string format, params object[] arguments)
    {
        if (Enabled)
        {
            Console.WriteLine("WARNING: " + format, arguments);
        }
    }

    public void Error(string message)
    {
        Console.WriteLine("ERROR: " + message);
    }

    public void Error(string format, params object[] arguments)
    {
        Console.WriteLine("ERROR: " + format, arguments);
    }

    public void Verbose(string message)
    {
        if (EnabledVerbose)
        {
            Console.WriteLine(message);
        }
    }

    public void Verbose(string format, params object[] arguments)
    {
        if (EnabledVerbose)
        {
            Console.WriteLine(format, arguments);
        }
    }
}