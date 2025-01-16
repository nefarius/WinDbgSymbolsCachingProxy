namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

public class FailedToParsePdbException(string message, int? age = null, string? indexPrefix = null) : Exception(message)
{
    public override string ToString()
    {
        return age is not null && indexPrefix is not null 
            ? $"{Message} (age: {age}, index: {indexPrefix})" 
            : Message;
    }
}