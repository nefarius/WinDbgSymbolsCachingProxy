namespace HarvestingAgent;

public sealed class AuthenticationConfig
{
    public required string Username { get; set; }

    public required string Password { get; set; }
}

public sealed class ServiceConfig
{
    public required AuthenticationConfig Authentication { get; set; }

    public required Uri ServerUrl { get; set; }
}