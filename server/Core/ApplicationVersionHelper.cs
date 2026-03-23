using System.Reflection;

using PeNet;
using PeNet.Header.Resource;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Resolves a human-readable server version from assembly metadata (MinVer / Docker-friendly) with PE fallback.
/// </summary>
public static class ApplicationVersionHelper
{
    /// <summary>
    ///     Returns the best display version for the entry assembly, or <c>null</c> if none is available.
    /// </summary>
    /// <remarks>
    ///     Prefer <see cref="AssemblyInformationalVersionAttribute"/> (set by MinVer and
    ///     <c>MINVERVERSIONOVERRIDE</c> in Docker) over Win32 <c>FileVersion</c> resources, which often remain
    ///     <c>0.0.0.0</c> for cross-platform / framework-dependent publishes.
    /// </remarks>
    public static string? TryGetServerVersion()
    {
        Assembly? asm = Assembly.GetEntryAssembly();
        if (asm is null)
            return null;

        string? informational = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informational) && !IsTrivialFourPartVersion(informational))
            return informational.Trim();

        string? fileVerAttr = asm.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        if (!string.IsNullOrWhiteSpace(fileVerAttr) && !IsTrivialFourPartVersion(fileVerAttr))
            return fileVerAttr.Trim();

        Version? nameVersion = asm.GetName().Version;
        if (nameVersion is not null && !IsAllZeros(nameVersion))
            return nameVersion.ToString();

        if (!string.IsNullOrEmpty(asm.Location))
        {
            try
            {
                PeFile peFile = new(asm.Location);
                StringTable? stringTable = peFile.Resources?.VsVersionInfo?.StringFileInfo.StringTable.FirstOrDefault();
                string? peFileVersion = stringTable?.FileVersion;
                if (!string.IsNullOrWhiteSpace(peFileVersion) && !IsTrivialFourPartVersion(peFileVersion))
                    return peFileVersion.Trim();
            }
            catch (Exception)
            {
                // PE parse can fail for some layouts; ignore
            }
        }

        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Trim();

        return nameVersion?.ToString();
    }

    /// <summary>
    ///     True when the string is only a trivial <c>0.0.0.0</c>-style version (ignores a <c>+metadata</c> suffix for the numeric part).
    /// </summary>
    private static bool IsTrivialFourPartVersion(string value)
    {
        value = value.Trim();
        int plus = value.IndexOf('+', StringComparison.Ordinal);
        string core = plus >= 0 ? value[..plus].Trim() : value;
        return Version.TryParse(core, out Version? v) && IsAllZeros(v);
    }

    private static bool IsAllZeros(Version v) =>
        v.Major == 0 && v.Minor == 0 && v.Build == 0 && v.Revision == 0;
}
