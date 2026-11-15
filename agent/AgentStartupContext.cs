namespace HarvestingAgent;

/// <summary>
///     Captures listen port at process start so the UI can warn when the saved port differs (restart required).
/// </summary>
public sealed class AgentStartupContext
{
    public int ListenPortAtStartup { get; init; }

    /// <summary>
    ///     Raw <c>urls</c> / <c>ASPNETCORE_URLS</c> value when that setting chooses the listen address.
    ///     Null when the process binds <c>127.0.0.1</c> and the saved listen port.
    /// </summary>
    public string? ListenUrlsOverride { get; init; }
}
