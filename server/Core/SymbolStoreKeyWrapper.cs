using Microsoft.SymbolStore;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Provides a wrapper for the <see cref="SymbolStoreKey" /> class, associating it with the input file
///     from which the key was generated. This is particularly useful for working with symbol store keys
///     in scenarios where the input file context needs to be retained.
/// </summary>
public sealed class SymbolStoreKeyWrapper
{
    /// <summary>The name of the input file from which the key was generated.</summary>
    public readonly string InputFile;

    /// <summary>The symbol store key (index prefix, etc.).</summary>
    public readonly SymbolStoreKey Key;

    internal SymbolStoreKeyWrapper(SymbolStoreKey key, string inputFile)
    {
        Key = key;
        InputFile = inputFile;
    }

    /// <summary>
    ///     Returns the hash of the index.
    /// </summary>
    public override int GetHashCode()
    {
        return Key.GetHashCode();
    }

    /// <summary>
    ///     Only the index is compared or hashed. The FileName is already
    ///     part of the index.
    /// </summary>
    public override bool Equals(object obj)
    {
        SymbolStoreKeyWrapper wrapper = (SymbolStoreKeyWrapper)obj;
        return Key.Equals(wrapper.Key);
    }
}