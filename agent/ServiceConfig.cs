using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace HarvestingAgent;

[UsedImplicitly]
public sealed class AuthenticationConfig : IEquatable<AuthenticationConfig>
{
    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public bool Equals(AuthenticationConfig? other)
    {
        return other is not null &&
               string.Equals(Username, other.Username, StringComparison.Ordinal) &&
               string.Equals(Password, other.Password, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AuthenticationConfig);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.Ordinal.GetHashCode(Username),
            StringComparer.Ordinal.GetHashCode(Password));
    }
}

[UsedImplicitly]
public sealed class ServerConfig : IEquatable<ServerConfig>
{
    /// <summary>
    ///     Upload server authentication details.
    /// </summary>
    public AuthenticationConfig Authentication { get; set; } = new();

    /// <summary>
    ///     The upload server URL.
    /// </summary>
    public Uri? ServerUrl { get; set; }

    /// <summary>
    ///     Optional label for UI; when set, shown instead of the URL in the dashboard file activity table.
    /// </summary>
    public string DisplayName { get; set; } = "";

    /// <summary>
    ///     Directories to watch. Use <see cref="WatcherPathEntry.IncludeSubdirectories" /> instead of a trailing <c>*</c> on the path.
    /// </summary>
    [JsonConverter(typeof(WatcherPathEntryListJsonConverter))]
    public List<WatcherPathEntry> WatcherPaths { get; set; } = [];

    /// <summary>
    ///     File name patterns to monitor (e.g. *.pdb). When empty, defaults to *.exe, *.dll, *.sys, *.pdb.
    /// </summary>
    public List<string> UploadFileFilters { get; set; } = [];

    /// <summary>
    ///     Gets whether the scraped and uploaded file should be deleted from disk on success.
    /// </summary>
    public bool DeleteAfterUpload { get; set; } = true;

    /// <summary>
    ///     An explicit filter on which files to include if <see cref="DeleteAfterUpload" /> is true. Accepts filesystem
    ///     globbing patterns.
    /// </summary>
    /// <remarks>See https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing</remarks>
    public List<string> DeletionInclusionFilter { get; set; } = ["**/*.pdb"];

    /// <summary>
    ///     An explicit filter on which files to exclude if <see cref="DeleteAfterUpload" /> is true. Accepts filesystem
    ///     globbing patterns.
    /// </summary>
    /// <remarks>See https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing</remarks>
    public List<string> DeletionExclusionFilter { get; set; } = [];

    public bool Equals(ServerConfig? other)
    {
        return other is not null &&
               EqualityComparer<Uri?>.Default.Equals(ServerUrl, other.ServerUrl) &&
               EqualityComparer<AuthenticationConfig>.Default.Equals(Authentication, other.Authentication) &&
               string.Equals(DisplayName, other.DisplayName, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ServerConfig);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(ServerUrl);
        hash.Add(Authentication);
        hash.Add(DisplayName, StringComparer.Ordinal);
        return hash.ToHashCode();
    }
}

public sealed class ServiceConfig
{
    public List<ServerConfig> Servers { get; set; } = [];
}
