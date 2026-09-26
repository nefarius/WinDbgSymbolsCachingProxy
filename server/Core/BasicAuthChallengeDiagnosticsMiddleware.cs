namespace WinDbgSymbolsCachingProxy.Core;

/// <summary>
///     Logs a single structured warning when the application issues a Basic-auth 401 challenge.
///     Successful symbol traffic is left unchanged.
/// </summary>
internal sealed class BasicAuthChallengeDiagnosticsMiddleware(
    RequestDelegate next,
    ILogger<BasicAuthChallengeDiagnosticsMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        await next(context);

        if (!BasicAuthChallengeDiagnostics.IsBasicChallenge(context))
        {
            return;
        }

        BasicAuthChallengeInfo info = BasicAuthChallengeDiagnostics.Capture(context);

        logger.LogWarning(
            BasicAuthChallengeDiagnostics.ChallengeMessageTemplate,
            info.Method,
            info.Path,
            info.EndpointDisplayName,
            info.AuthorizationHeaderPresent,
            info.RemoteIp,
            info.UserAgent,
            info.TraceId);
    }
}
