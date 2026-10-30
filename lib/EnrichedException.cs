using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Nefarius.Utilities.ExceptionEnricher;

/// <summary>
///     A custom-built exception with debug information resolved from other sources than local files.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class EnrichedException : Exception
{
    private readonly string _message;
    private readonly string _stackTrace;

    internal EnrichedException(Exception originalException, string stackTrace)
        : base(originalException.Message, originalException.InnerException)
    {
        OriginalException = originalException;
        OriginalType = originalException.GetType();

        // carry over the other interesting exception details
        HResult = originalException.HResult;
        Source = originalException.Source;
        HelpLink = originalException.HelpLink;
        foreach (DictionaryEntry entry in originalException.Data)
        {
            Data[entry.Key] = entry.Value;
        }

        _message = originalException.Message;
        _stackTrace = stackTrace;
    }

    /// <summary>
    ///     Gets the <see cref="Exception" /> this <see cref="EnrichedException" /> was created from.
    /// </summary>
    public Exception OriginalException { get; }

    /// <summary>
    ///     Gets the <see cref="Type" /> this <see cref="EnrichedException" /> is based on.
    /// </summary>
    public Type OriginalType { get; }

    /// <inheritdoc />
    public override string Message => _message;

    /// <inheritdoc />
    public override string StackTrace => _stackTrace;

    /// <inheritdoc />
    public override string ToString()
    {
        return base.ToString().Replace(GetType().ToString(), OriginalType.ToString());
    }
}
