namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

public class FailedToParseExecutableException : Exception
{
    public FailedToParseExecutableException(string message) : base(message) { }
}