namespace WinDbgSymbolsCachingProxy.Models;

internal sealed record SymbolParsingResult(
    string FileName,
    string IndexPrefix,
    uint? Age = null,
    ulong? Signature = null,
    Guid? NewSignature = null)
{
    /// <summary>
    ///     A hex-encoded concatenated string of the Signature and Age where the Signature in "modern" PDB v7 symbols is
    ///     represented in a GUID format and the Age is an unsigned 32-bit integer without leading zeroes.
    /// </summary>
    public string SymbolKey
    {
        get
        {
            if (!Age.HasValue)
            {
                return IndexPrefix.Split('/')[1].ToUpperInvariant();
            }

            if (!Signature.HasValue && !NewSignature.HasValue)
            {
                return IndexPrefix.Split('/')[1].ToUpperInvariant();
            }

            if (NewSignature.HasValue)
            {
                return $"{NewSignature.Value:N}{Age.Value:X}".ToUpperInvariant();
            }

            return Signature.HasValue
                ? $"{Signature.Value:X}{Age.Value:X}".ToUpperInvariant()
                : IndexPrefix.Split('/')[1].ToUpperInvariant();
        }
    }
}