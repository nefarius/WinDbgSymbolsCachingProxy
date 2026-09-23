namespace WinDbgSymbolsCachingProxy.UpdateCheck;

/// <summary>
///     Supplies the version of the running application when no MSI installation is present.
/// </summary>
public interface ILocalAssemblyVersionSource
{
    string? TryGetVersion();
}
