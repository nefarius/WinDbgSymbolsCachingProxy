namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

/// <summary>
///     Thrown when parsing an executable (.exe, .dll, .sys) fails or no symbol keys can be generated.
/// </summary>
public class FailedToParseExecutableException(string message) : Exception(message);