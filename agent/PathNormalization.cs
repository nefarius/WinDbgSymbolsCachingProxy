namespace HarvestingAgent;

public static class PathNormalization
{
    /// <summary>
    /// Normalize a directory path into a canonical, platform-specific full path.
    /// </summary>
    /// <param name="raw">The input path to normalize. If null, empty, or whitespace only, the method returns an empty string.</param>
    /// <returns>
    /// A normalized path that uses the platform directory separator, collapses repeated separators, removes trailing separators except for root paths, and preserves a Windows UNC prefix when present; or an empty string if <paramref name="raw"/> is null/empty/whitespace.
    /// </returns>
    public static string NormalizeDirectoryPath(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        string trimmed = raw.Trim();
        char ds = Path.DirectorySeparatorChar;
        char ads = Path.AltDirectorySeparatorChar;

        // Normalize separators first so behavior is predictable per host OS.
        string normalized = trimmed
            .Replace(ads, ds)
            .Replace(ds == '\\' ? '/' : '\\', ds);

        bool isUnc = OperatingSystem.IsWindows() && normalized.StartsWith(new string(ds, 2), StringComparison.Ordinal);
        string root = Path.GetPathRoot(normalized) ?? string.Empty;

        if (!string.IsNullOrEmpty(root))
        {
            string rest = normalized[root.Length..];
            while (rest.Contains(new string(ds, 2), StringComparison.Ordinal))
            {
                rest = rest.Replace(new string(ds, 2), ds.ToString(), StringComparison.Ordinal);
            }

            normalized = root + rest;
        }
        else
        {
            while (normalized.Contains(new string(ds, 2), StringComparison.Ordinal))
            {
                normalized = normalized.Replace(new string(ds, 2), ds.ToString(), StringComparison.Ordinal);
            }
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(normalized);
        }
        catch
        {
            fullPath = normalized;
        }

        string fullRoot = Path.GetPathRoot(fullPath) ?? string.Empty;
        if (!string.Equals(fullPath, fullRoot, StringComparison.Ordinal))
        {
            fullPath = fullPath.TrimEnd(ds);
        }

        // Preserve UNC prefix if available and not a root-only path.
        if (isUnc && !fullPath.StartsWith(new string(ds, 2), StringComparison.Ordinal))
        {
            fullPath = $"{ds}{ds}{fullPath.TrimStart(ds)}";
        }

        return fullPath;
    }
}
