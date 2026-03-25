using System.Reflection;

namespace HarvestingAgent;

/// <summary>
/// Resolves a human-readable version for the running agent assembly.
/// </summary>
public static class ApplicationVersionHelper
{
    /// <summary>
    /// Returns the best display version for the entry assembly, or <c>null</c> if none is available.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="AssemblyInformationalVersionAttribute"/> (set by MinVer / Docker tags)
    /// over Win32 <c>FileVersion</c> resources, which often remain <c>0.0.0.0</c> for some publishes.
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

        // Fall back to informational even if it is trivial, because it is still "real" metadata.
        if (!string.IsNullOrWhiteSpace(informational))
            return informational.Trim();

        return nameVersion?.ToString();
    }

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

