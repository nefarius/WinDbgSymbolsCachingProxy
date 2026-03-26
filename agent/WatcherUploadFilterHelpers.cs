namespace HarvestingAgent;

/// <summary>
///     Shared logic for effective per-watched-path upload filters and stable keys used by
///     <see cref="HarvesterRuntime.RebuildWatchersAsync" /> and overlap detection in the configuration UI.
/// </summary>
public static class WatcherUploadFilterHelpers
{
    public static readonly string[] DefaultUploadFilters = ["*.exe", "*.dll", "*.sys", "*.pdb"];

    public static IReadOnlyList<string> GetEffectiveUploadFilters(WatcherPathEntry watch)
    {
        return watch.UploadFileFilters.Count > 0 ? watch.UploadFileFilters : DefaultUploadFilters;
    }

    /// <summary>
    ///     Normalized key for grouping watchers; must match <see cref="HarvesterRuntime" /> grouping.
    /// </summary>
    public static string NormalizeFiltersKey(IReadOnlyList<string> patterns)
    {
        return string.Join(
            "\n",
            patterns
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase));
    }
}
