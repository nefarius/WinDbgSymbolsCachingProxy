using System.Globalization;
using System.Runtime.Versioning;

using Microsoft.Win32;

namespace WinDbgSymbolsCachingProxy.UpdateCheck;

/// <summary>
///     One Programs and Features entry that may represent the installed product.
/// </summary>
public readonly record struct InstalledProductRecord(
    string? DisplayName,
    string? Publisher,
    string? DisplayVersion,
    bool IsWindowsInstaller);

/// <summary>
///     Reads the installed MSI product version. Returns <c>null</c> when it cannot be determined.
/// </summary>
public interface IInstalledMsiVersionSource
{
    string? TryGetVersion();
}

/// <summary>
///     Selects the WinDbg Symbols Caching Proxy MSI from uninstall entries.
/// </summary>
public static class InstalledMsiVersionSelector
{
    public const string ProductName = "WinDbg Symbols Caching Proxy";

    public const string Manufacturer = "Nefarius Software Solutions";

    public static string? Select(IEnumerable<InstalledProductRecord> products)
    {
        foreach (InstalledProductRecord product in products)
        {
            if (!product.IsWindowsInstaller)
                continue;

            if (!string.Equals(product.DisplayName?.Trim(), ProductName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!string.Equals(product.Publisher?.Trim(), Manufacturer, StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrWhiteSpace(product.DisplayVersion))
                continue;

            return product.DisplayVersion.Trim();
        }

        return null;
    }
}

/// <summary>
///     Reads the per-machine MSI uninstall registry entries for this product.
/// </summary>
public sealed class WindowsInstalledMsiVersionSource : IInstalledMsiVersionSource
{
    private const string UninstallKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    public string? TryGetVersion()
    {
        if (!OperatingSystem.IsWindows())
            return null;

        try
        {
            return InstalledMsiVersionSelector.Select(ReadUninstallEntries());
        }
        catch (Exception)
        {
            return null;
        }
    }

    [SupportedOSPlatform("windows")]
    private static List<InstalledProductRecord> ReadUninstallEntries()
    {
        List<InstalledProductRecord> products = [];
        RegistryView[] views = [RegistryView.Registry64, RegistryView.Registry32];
        foreach (RegistryView view in views)
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using RegistryKey? uninstall = baseKey.OpenSubKey(UninstallKeyPath);
            if (uninstall is null)
                continue;

            foreach (string subKeyName in uninstall.GetSubKeyNames())
            {
                using RegistryKey? product = uninstall.OpenSubKey(subKeyName);
                if (product is null)
                    continue;

                products.Add(new InstalledProductRecord(
                    ReadString(product, "DisplayName"),
                    ReadString(product, "Publisher"),
                    ReadString(product, "DisplayVersion"),
                    IsWindowsInstaller(product)));
            }
        }

        return products;
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadString(RegistryKey key, string name)
    {
        object? value = key.GetValue(name);
        return value switch
        {
            string text => text,
            null => null,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture)
        };
    }

    [SupportedOSPlatform("windows")]
    private static bool IsWindowsInstaller(RegistryKey key)
    {
        object? value = key.GetValue("WindowsInstaller");
        return value switch
        {
            int flag => flag == 1,
            string text => text == "1",
            _ => false
        };
    }
}
