namespace WinDbgSymbolsCachingProxy.Models;

/// <summary>
///     Result of parsing a symbol file: file name, index prefix, and optional PDB signature/age.
/// </summary>
/// <param name="FileName">The symbol file name (e.g. module.pdb).</param>
/// <param name="IndexPrefix">The index prefix path (e.g. "module.pdb/GUIDAGE/").</param>
/// <param name="Age">The PDB age value, if applicable.</param>
/// <param name="Signature">The pre-v7 PDB signature, if applicable.</param>
/// <param name="NewSignature">The v7 PDB GUID signature, if applicable.</param>
internal sealed record SymbolParsingResult(
    string FileName,
    string IndexPrefix,
    uint? Age = null,
    ulong? Signature = null,
    Guid? NewSignature = null)
{
    /// <summary>
    ///     A hex-encoded concatenated string of the Signature and Age where the Signature in "modern" PDB v7 symbols is
    ///     represented in a GUID format, and the Age is an unsigned 32-bit integer without leading zeroes.
    /// </summary>
    public string SymbolKey
    {
        get
        {
            if (!Age.HasValue || !Signature.HasValue && !NewSignature.HasValue)
            {
                return IndexPrefix.Split('/')[1].ToUpperInvariant();
            }

            if (NewSignature.HasValue)
            {
                return $"{NewSignature.Value:N}{Age.Value:X}".ToUpperInvariant();
            }

            return $"{Signature!.Value:X}{Age.Value:X}".ToUpperInvariant();
        }
    }
}