using System;
using System.Diagnostics.CodeAnalysis;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     A custom-built exception with debug information resolved from other sources than local files.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class EnrichedException : Exception
{
    internal EnrichedException(Exception originalException, string stackTrace)
    {
        BaseException = originalException;
        StackTrace = stackTrace;
    }

    /// <summary>
    ///     The <see cref="Exception" /> object this <see cref="EnrichedException" /> is based on.
    /// </summary>
    public Exception BaseException { get; }

    /// <inheritdoc />
    public override string Message => BaseException.Message;

    /// <inheritdoc />
    public override string StackTrace { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return BaseException.ToString();
    }

    /// <inheritdoc />
    public override Exception GetBaseException()
    {
        return BaseException;
    }
}