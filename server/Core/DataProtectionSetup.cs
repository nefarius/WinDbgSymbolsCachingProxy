using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Resolves and prepares the Data Protection key directory.
/// </summary>
internal static class DataProtectionSetup
{
    private const string CompanyFolderName = "WinDbgSymbolsCachingProxy";
    private const string ServerFolderName = "Server";

    /// <summary>
    ///     Returns the (created) key directory: the configured path, else <c>%ProgramData%</c> on Windows,
    ///     else a <c>dp-keys</c> folder next to the application.
    /// </summary>
    public static DirectoryInfo ResolveKeysDirectory(string? configuredPath)
    {
        string path;
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            path = Path.GetFullPath(configuredPath);
        }
        else if (OperatingSystem.IsWindows())
        {
            path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                CompanyFolderName, ServerFolderName, "dp-keys");
        }
        else
        {
            path = Path.Combine(AppContext.BaseDirectory, "dp-keys");
        }

        DirectoryInfo dir = new(path);
        bool existed = dir.Exists;
        dir.Create();

        if (OperatingSystem.IsWindows())
        {
            if (!existed)
            {
                RestrictToAdministrators(dir);
            }
            else
            {
                // The service account can change between installs (LocalService <-> NetworkService); keep existing
                // rules (SYSTEM, Administrators, ...) and make sure both supported accounts can read/write the keys.
                GrantServiceAccounts(dir);
            }
        }

        return dir;
    }

    [SupportedOSPlatform("windows")]
    private static void GrantServiceAccounts(DirectoryInfo dir)
    {
        try
        {
            DirectorySecurity security = dir.GetAccessControl();
            const InheritanceFlags inherit = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            foreach (SecurityIdentifier sid in new[]
                     {
                         new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null),
                         new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null)
                     })
            {
                security.AddAccessRule(new FileSystemAccessRule(
                    sid, FileSystemRights.Modify, inherit, PropagationFlags.None, AccessControlType.Allow));
            }

            dir.SetAccessControl(security);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            Serilog.Log.Warning(ex, "Could not grant service accounts access to {Directory}", dir.FullName);
        }
    }

    /// <summary>
    ///     Replaces the inherited ACL with full control for SYSTEM, Administrators, the current account and the two supported service accounts.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static void RestrictToAdministrators(DirectoryInfo dir)
    {
        try
        {
            DirectorySecurity security = new();
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);

            const InheritanceFlags inherit = InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit;
            foreach (SecurityIdentifier sid in new[]
                     {
                         new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                         new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                         WindowsIdentity.GetCurrent().User!,
                         new SecurityIdentifier(WellKnownSidType.LocalServiceSid, null),
                         new SecurityIdentifier(WellKnownSidType.NetworkServiceSid, null)
                     })
            {
                security.AddAccessRule(new FileSystemAccessRule(
                    sid, FileSystemRights.FullControl, inherit, PropagationFlags.None, AccessControlType.Allow));
            }

            dir.SetAccessControl(security);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            // Hardening is best effort; the keys remain DPAPI-protected regardless.
            Serilog.Log.Warning(ex, "Could not restrict permissions on {Directory}", dir.FullName);
        }
    }
}
