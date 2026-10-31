using System.Security.AccessControl;
using System.Security.Principal;

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

    /// <summary>
    ///     The settings file stores upload credentials and API keys; limit the directory to SYSTEM, Administrators and the
    ///     account running the agent instead of the default ProgramData inheritance (which grants all users read access).
    /// </summary>
    public static void RestrictSettingsDirectory(string directory)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            DirectoryInfo dir = new(directory);
            DirectorySecurity security = new();
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);

            const InheritanceFlags inherit = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            foreach (SecurityIdentifier sid in new[]
                     {
                         new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                         new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                         WindowsIdentity.GetCurrent().User!
                     })
            {
                security.AddAccessRule(new FileSystemAccessRule(
                    sid, FileSystemRights.FullControl, inherit, PropagationFlags.None, AccessControlType.Allow));
            }

            dir.SetAccessControl(security);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            Console.Error.WriteLine($"[HarvestingAgent] Could not restrict permissions on {directory}: {ex.Message}");
        }
    }

    private static string SettingsRoot =>
        OperatingSystem.IsWindows()
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
}
