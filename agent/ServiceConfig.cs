namespace HarvestingAgent;

public sealed class AuthenticationConfig
{
    public required string Username { get; set; }

    public required string Password { get; set; }
}

public sealed class ServerConfig
{
    /// <summary>
    ///     Upload server authentication details.
    /// </summary>
    public required AuthenticationConfig Authentication { get; set; }

    /// <summary>
    ///     The upload server URL.
    /// </summary>
    public required Uri ServerUrl { get; set; }

    /// <summary>
    ///     A collection of filesystem paths to watch over for changes.
    /// </summary>
    public required List<string> WatcherPaths { get; set; }

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
}

public sealed class ServiceConfig
{
    public required List<ServerConfig> Servers { get; set; } = [];
}