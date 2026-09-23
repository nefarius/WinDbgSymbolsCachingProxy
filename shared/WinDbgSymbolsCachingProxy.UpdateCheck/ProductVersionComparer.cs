using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace WinDbgSymbolsCachingProxy.UpdateCheck;

/// <summary>
///     Compares product versions using the same four-part numeric form written into the MSI.
/// </summary>
public static class ProductVersionComparer
{
    /// <summary>
    ///     Parses a product version, ignoring SemVer pre-release and build metadata suffixes.
    ///     Missing numeric parts are treated as zero so <c>1.31.2</c> and <c>1.31.2.0</c> compare equal.
    /// </summary>
    public static bool TryParse(string? raw, [NotNullWhen(true)] out Version? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        string numeric = raw.Trim().Split('+', 2)[0].Split('-', 2)[0].Trim();
        string[] parts = numeric.Split('.');
        if (parts.Length is 0 or > 4)
            return false;

        int[] numbers = new int[4];
        for (int index = 0; index < parts.Length; index++)
        {
            if (!int.TryParse(parts[index], NumberStyles.None, CultureInfo.InvariantCulture, out int number) ||
                number < 0)
                return false;

            numbers[index] = number;
        }

        version = new Version(numbers[0], numbers[1], numbers[2], numbers[3]);
        return true;
    }

    /// <summary>
    ///     Returns whether <paramref name="candidate"/> is a newer numeric product version than <paramref name="baseline"/>.
    /// </summary>
    public static bool IsNewer(string? candidate, string? baseline)
    {
        if (!TryParse(candidate, out Version? latest) || !TryParse(baseline, out Version? local))
            return false;

        return latest > local;
    }
}
