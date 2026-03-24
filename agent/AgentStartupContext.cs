namespace HarvestingAgent;

/// <summary>
///     Captures listen port at process start so the UI can warn when the saved port differs (restart required).
/// </summary>
public sealed class AgentStartupContext
{
    public int ListenPortAtStartup { get; init; }
}
