namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

/// <summary>
///     Thrown when the provided file has an extension that is not supported for symbol parsing (e.g. not .pdb, .exe, .dll, .sys).
/// </summary>
public sealed class UnsupportedFileTypeException(string message) : Exception(message);