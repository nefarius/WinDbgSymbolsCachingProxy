namespace HarvestingAgent;

/// <summary>
///     Root document persisted to ProgramData as JSON.
/// </summary>
public sealed class AgentSettingsDocument
{
    public int ListenPort { get; set; } = 5088;

    public bool HarvestingEnabled { get; set; } = true;

    public List<ServerConfig> Servers { get; set; } = [];

    public ServiceConfig ToServiceConfig() => new() { Servers = Servers };

    public static AgentSettingsDocument CreateDefault() => new();
}
