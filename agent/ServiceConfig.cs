namespace HarvestingAgent;

public sealed class AuthenticationConfig
{
    public required string Username { get; set; }

    public required string Password { get; set; }
}

public sealed class ServerConfig
{
    public required AuthenticationConfig Authentication { get; set; }

    public required Uri ServerUrl { get; set; }

    public required List<string> WatcherPaths { get; set; }

    public bool DeleteAfterUpload { get; set; } = true;

    public List<string> DeletionInclusionFilter { get; set; } = ["**/*.pdb"];
}

public sealed class ServiceConfig
{
    public required List<ServerConfig> Servers { get; set; } = [];
}