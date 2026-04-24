namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

/// <summary>
///     Thrown when an uploaded symbol file is missing the magic signature expected for its extension.
///     Typically indicates a truncated, zero-filled, or still-being-written file; the sender should wait
///     until the file is fully flushed before retrying.
/// </summary>
public sealed class IncompleteSymbolFileException(string message) : Exception(message);
