using System;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     A custom-built exception with debug information resolved from other sources than local files.
/// </summary>
public class EnrichedException : Exception
{
    internal EnrichedException(string stackTrace)
    {
        StackTrace = stackTrace;
    }

    /// <inheritdoc />
    public override string StackTrace { get; }
}