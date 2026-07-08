namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Well-known permission strings used as policy names and claim values.
/// </summary>
public static class Permissions
{
    public const string SymbolsDownload = "symbols.download";
    public const string SymbolsUpload = "symbols.upload";
    public const string SymbolsDelete = "symbols.delete";
    public const string LogsView = "logs.view";
    public const string Admin = "admin";

    /// <summary>All defined permissions in definition order.</summary>
    public static readonly string[] All =
        [SymbolsDownload, SymbolsUpload, SymbolsDelete, LogsView, Admin];
}
