using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace WinDbgSymbolsCachingProxy.Models;

#nullable disable

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
public sealed class SymbolsRequest
{
    /// <summary>
    ///     The symbol name (left-hand part of the so-called "key" of a symbol).
    /// </summary>
    public string Symbol { get; set; }

    /// <summary>
    ///     A string of the two values "Signature" and "Age" as concatenated hex-values.
    /// </summary>
    public string SignatureAge { get; set; }

    /// <summary>
    ///     The requested file name. This can be the symbol itself or a text file containing a "redirect path" where the symbol
    ///     can be found.
    /// </summary>
    public string File { get; set; }

    /// <summary>
    ///     Gets what is called the "key" of the symbol which uniquely identifies it.
    /// </summary>
    [JsonIgnore]
    public string Key => $"{Symbol}/{SignatureAge}";
}