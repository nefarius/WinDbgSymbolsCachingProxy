namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Limits and filters for symbol uploads (aligned with Kestrel body limit and server-side symbol parsing).
/// </summary>
public static class SymbolUploadConstants
{
    /// <summary>Matches Kestrel <c>MaxRequestBodySize</c> in <c>Program.cs</c>.</summary>
    public const long MaxUploadBytesPerFile = 200_000_000;

    /// <summary>HTML <c>accept</c> attribute for the file picker.</summary>
    public const string AcceptAttribute = ".pdb,.exe,.dll,.sys";

    public static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdb",
        ".exe",
        ".dll",
        ".sys"
    };
}
