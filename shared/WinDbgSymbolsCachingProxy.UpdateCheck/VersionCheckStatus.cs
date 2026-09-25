namespace WinDbgSymbolsCachingProxy.UpdateCheck;

/// <summary>
///     Describes where the version shown in the app bar came from.
/// </summary>
public enum LocalVersionOrigin
{
    None = 0,
    InstalledMsi = 1,
    Assembly = 2
}

/// <summary>
///     Local product version compared with the latest published MSI.
/// </summary>
public sealed record VersionCheckStatus(
    string? LocalVersion,
    LocalVersionOrigin Origin,
    string? LatestVersion,
    bool UpdateAvailable,
    Uri? DownloadUri)
{
    public bool HasLocalVersion => !string.IsNullOrWhiteSpace(LocalVersion);

    public string ChipLabel =>
        UpdateAvailable && !string.IsNullOrWhiteSpace(LatestVersion)
            ? $"{LocalVersion} · {LatestVersion} available"
            : LocalVersion ?? string.Empty;

    public string Tooltip
    {
        get
        {
            if (!HasLocalVersion)
                return "Version unavailable";

            string source = Origin == LocalVersionOrigin.InstalledMsi
                ? "Installed MSI version"
                : "Running version";

            if (UpdateAvailable && !string.IsNullOrWhiteSpace(LatestVersion))
                return $"{source} {LocalVersion}. Version {LatestVersion} is available.";

            return $"{source} {LocalVersion}.";
        }
    }
}
