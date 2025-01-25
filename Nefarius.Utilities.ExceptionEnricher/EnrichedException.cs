using System;
using System.Diagnostics.CodeAnalysis;

using FastDeepCloner;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     A custom-built exception with debug information resolved from other sources than local files.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class EnrichedException : Exception
{
    internal EnrichedException(Exception originalException, string stackTrace)
    {
        originalException.CloneTo(this);

        Message = originalException.Message;
        StackTrace = stackTrace;
    }

    /// <inheritdoc />
    public override string Message { get; }

    /// <inheritdoc />
    public override string StackTrace { get; }
}