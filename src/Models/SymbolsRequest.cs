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
    public string SymbolKey { get; set; }

    /// <summary>
    ///     The requested file name. This can be the symbol itself or a text file containing a "redirect path" where the symbol
    ///     can be found.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    ///     The index prefix is built as a relative URL segment from <see cref="Symbol"/> and <see cref="SymbolKey"/>.
    /// </summary>
    [JsonIgnore]
    public string IndexPrefix => $"{Symbol}/{SymbolKey.ToUpperInvariant()}/";
}