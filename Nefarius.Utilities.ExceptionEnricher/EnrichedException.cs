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
        OriginalType = originalException.GetType();

        // this way we conveniently keep all the other interesting exception details
        originalException.CloneTo(this);

        Message = originalException.Message;
        StackTrace = stackTrace;
    }

    /// <summary>
    ///     Gets the <see cref="Type" /> this <see cref="EnrichedException" /> is based on.
    /// </summary>
    public Type OriginalType { get; }

    /// <inheritdoc />
    public override string Message { get; }

    /// <inheritdoc />
    public override string StackTrace { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return base.ToString().Replace(GetType().ToString(), OriginalType.ToString());
    }
}