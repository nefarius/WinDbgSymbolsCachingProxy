using System.Reflection;

using PeNet;
using PeNet.Header.Resource;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Reads the Windows PE file version string for the entry assembly (same source as <c>/info</c>).
/// </summary>
public static class ApplicationVersionHelper
{
    /// <summary>
    ///     Returns the file version from the entry assembly's version resources, or <c>null</c> if it cannot be read.
    /// </summary>
    public static string? TryGetEntryAssemblyFileVersion()
    {
        Assembly? asm = Assembly.GetEntryAssembly();
        if (string.IsNullOrEmpty(asm?.Location))
            return null;

        PeFile peFile = new(asm.Location);
        if (peFile.Resources is null)
            return null;

        StringTable? stringTable = peFile.Resources.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault();
        return stringTable?.FileVersion;
    }
}
