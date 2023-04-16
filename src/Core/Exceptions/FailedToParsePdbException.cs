namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

public class FailedToParsePdbException : Exception
{
    public FailedToParsePdbException(string message) : base(message) { }
}