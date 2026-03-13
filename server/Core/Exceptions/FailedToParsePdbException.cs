namespace WinDbgSymbolsCachingProxy.Core.Exceptions;

/// <summary>
///     Thrown when parsing a PDB file fails or the PDB version is unsupported.
/// </summary>
public class FailedToParsePdbException(string message, int? age = null, string? indexPrefix = null) : Exception(message)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return age is not null && indexPrefix is not null 
            ? $"{Message} (age: {age}, index: {indexPrefix})" 
            : Message;
    }
}