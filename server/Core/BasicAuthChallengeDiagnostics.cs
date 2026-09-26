namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Safe, structured context for Basic-auth challenge and credential-rejection logs.
///     Never includes Authorization header values, usernames, or passwords.
/// </summary>
internal sealed record BasicAuthChallengeInfo(
    string Method,
    string Path,
    string? EndpointDisplayName,
    string? RemoteIp,
    string? UserAgent,
    string TraceId,
    bool AuthorizationHeaderPresent,
    bool UsernamePresent);

/// <summary>
///     Detects app-generated Basic authentication challenges and captures request context
///     without recording credential material.
/// </summary>
internal static class BasicAuthChallengeDiagnostics
{
    internal const string ChallengeMessageTemplate =
        "Basic authentication challenge issued for {Method} {Path} (endpoint={Endpoint}, authorizationHeaderPresent={AuthorizationHeaderPresent}, remoteIp={RemoteIp}, userAgent={UserAgent}, traceId={TraceId})";

    internal const string RejectionMessageTemplate =
        "Basic authentication credentials rejected for {Method} {Path} (usernamePresent={UsernamePresent}, authorizationHeaderPresent={AuthorizationHeaderPresent}, remoteIp={RemoteIp}, userAgent={UserAgent}, traceId={TraceId})";

    private const int UserAgentMaxLength = 256;

    /// <summary>
    ///     Returns true when the response is a 401 carrying a Basic <c>WWW-Authenticate</c> challenge.
    /// </summary>
    public static bool IsBasicChallenge(int statusCode, IEnumerable<string>? wwwAuthenticateValues)
    {
        if (statusCode != StatusCodes.Status401Unauthorized || wwwAuthenticateValues is null)
        {
            return false;
        }

        foreach (string value in wwwAuthenticateValues)
        {
            if (value.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     True when the completed response is an app-generated Basic challenge.
    /// </summary>
    public static bool IsBasicChallenge(HttpContext context)
        => IsBasicChallenge(context.Response.StatusCode, context.Response.Headers.WWWAuthenticate);

    /// <summary>
    ///     Captures request metadata for a challenge or credential-rejection log line.
    ///     Copies only header presence, never Authorization values or credential strings.
    /// </summary>
    public static BasicAuthChallengeInfo Capture(HttpContext context, bool usernamePresent = false)
    {
        string? userAgent = context.Request.Headers.UserAgent.ToString();
        if (string.IsNullOrEmpty(userAgent))
        {
            userAgent = null;
        }
        else if (userAgent.Length > UserAgentMaxLength)
        {
            userAgent = userAgent[..UserAgentMaxLength];
        }

        return new BasicAuthChallengeInfo(
            context.Request.Method,
            context.Request.Path.HasValue ? context.Request.Path.Value! : "/",
            context.GetEndpoint()?.DisplayName,
            context.Connection.RemoteIpAddress?.ToString(),
            userAgent,
            context.TraceIdentifier,
            context.Request.Headers.ContainsKey("Authorization"),
            usernamePresent);
    }

    /// <summary>
    ///     Renders the challenge message the same way Serilog/MEL would, for redaction tests.
    /// </summary>
    public static string RenderChallengeMessage(BasicAuthChallengeInfo info)
        => $"Basic authentication challenge issued for {info.Method} {info.Path} (endpoint={info.EndpointDisplayName}, authorizationHeaderPresent={info.AuthorizationHeaderPresent}, remoteIp={info.RemoteIp}, userAgent={info.UserAgent}, traceId={info.TraceId})";

    /// <summary>
    ///     Renders the credential-rejection message the same way Serilog/MEL would, for redaction tests.
    /// </summary>
    public static string RenderRejectionMessage(BasicAuthChallengeInfo info)
        => $"Basic authentication credentials rejected for {info.Method} {info.Path} (usernamePresent={info.UsernamePresent}, authorizationHeaderPresent={info.AuthorizationHeaderPresent}, remoteIp={info.RemoteIp}, userAgent={info.UserAgent}, traceId={info.TraceId})";
}
