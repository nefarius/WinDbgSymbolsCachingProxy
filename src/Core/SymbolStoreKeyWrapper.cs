using Microsoft.SymbolStore;

namespace WinDbgSymbolsCachingProxy.Core;

internal sealed class SymbolStoreKeyWrapper
{
    public readonly string InputFile;
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