namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

/// <summary>
///     Thrown when an uploaded symbol file is missing the magic signature expected for its extension.
///     Typically indicates a truncated, zero-filled, or still-being-written file; the sender should wait
///     until the file is fully flushed before retrying.
/// </summary>
public sealed class IncompleteSymbolFileException : Exception
{
    public IncompleteSymbolFileException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Preserves the low-level parser exception (e.g. <see cref="OverflowException" /> from
    ///     Microsoft.FileFormats) so callers inspecting the exception chain get the full diagnostic
    ///     stack, not just the user-facing message.
    /// </summary>
    public IncompleteSymbolFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
