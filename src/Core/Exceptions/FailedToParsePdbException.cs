namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

public class FailedToParsePdbException(string message) : Exception(message);