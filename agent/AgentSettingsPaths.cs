namespace HarvestingAgent;

public static class AgentSettingsPaths
{
    public const string CompanyFolderName = "WinDbgSymbolsCachingProxy";
    public const string AgentFolderName = "HarvestingAgent";
    public const string SettingsFileName = "agent-settings.json";

    /// <summary>
    ///     Windows (production): under <c>%ProgramData%</c> so the service can run from Program Files without writing there.
    ///     Linux/macOS (local dev): under the user’s <see cref="Environment.SpecialFolder.LocalApplicationData" /> (no root).
    /// </summary>
    public static string SettingsDirectory =>
        Path.Combine(SettingsRoot, CompanyFolderName, AgentFolderName);

    public static string SettingsFilePath => Path.Combine(SettingsDirectory, SettingsFileName);

    private static string SettingsRoot =>
        OperatingSystem.IsWindows()
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
}
