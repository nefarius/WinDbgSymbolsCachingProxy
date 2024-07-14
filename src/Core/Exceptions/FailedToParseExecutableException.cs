namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

public class FailedToParseExecutableException(string message) : Exception(message);