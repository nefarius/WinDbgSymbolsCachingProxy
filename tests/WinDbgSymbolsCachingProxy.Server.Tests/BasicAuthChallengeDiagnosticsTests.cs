using System.Net;

using Microsoft.AspNetCore.Http;

using WinDbgSymbolsCachingProxy.Core;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public sealed class BasicAuthChallengeDiagnosticsTests
{
    private const string BasicPayload = "dXNlcjpwYXNzd29yZA=="; // user:password
    private const string AuthorizationValue = $"Basic {BasicPayload}";

    [Fact]
    public void IsBasicChallenge_detects_401_with_basic_www_authenticate()
    {
        Assert.True(BasicAuthChallengeDiagnostics.IsBasicChallenge(
            StatusCodes.Status401Unauthorized,
            ["Basic realm=\"Basic Authentication\""]));
    }

    [Fact]
    public void IsBasicChallenge_is_case_insensitive_for_scheme()
    {
        Assert.True(BasicAuthChallengeDiagnostics.IsBasicChallenge(
            StatusCodes.Status401Unauthorized,
            ["basic realm=\"x\""]));
    }

    [Theory]
    [InlineData(StatusCodes.Status200OK, "Basic realm=\"Basic Authentication\"")]
    [InlineData(StatusCodes.Status403Forbidden, "Basic realm=\"Basic Authentication\"")]
    [InlineData(StatusCodes.Status421MisdirectedRequest, "Basic realm=\"Basic Authentication\"")]
    [InlineData(StatusCodes.Status401Unauthorized, "Bearer realm=\"x\"")]
    public void IsBasicChallenge_suppresses_non_basic_or_non_401(int statusCode, string wwwAuthenticate)
    {
        Assert.False(BasicAuthChallengeDiagnostics.IsBasicChallenge(statusCode, [wwwAuthenticate]));
    }

    [Fact]
    public void IsBasicChallenge_suppresses_missing_www_authenticate()
    {
        Assert.False(BasicAuthChallengeDiagnostics.IsBasicChallenge(StatusCodes.Status401Unauthorized, null));
        Assert.False(BasicAuthChallengeDiagnostics.IsBasicChallenge(StatusCodes.Status401Unauthorized, []));
    }

    [Fact]
    public void Capture_includes_request_context_and_header_presence_only()
    {
        DefaultHttpContext context = CreateContext(
            method: "GET",
            path: "/auth-challenge",
            authorization: AuthorizationValue,
            userAgent: "WinDbg/1.0",
            remoteIp: "203.0.113.10",
            traceId: "00-trace",
            endpointDisplayName: "GET /auth-challenge");

        BasicAuthChallengeInfo info = BasicAuthChallengeDiagnostics.Capture(context, usernamePresent: true);

        Assert.Equal("GET", info.Method);
        Assert.Equal("/auth-challenge", info.Path);
        Assert.Equal("GET /auth-challenge", info.EndpointDisplayName);
        Assert.Equal("203.0.113.10", info.RemoteIp);
        Assert.Equal("WinDbg/1.0", info.UserAgent);
        Assert.Equal("00-trace", info.TraceId);
        Assert.True(info.AuthorizationHeaderPresent);
        Assert.True(info.UsernamePresent);
    }

    [Fact]
    public void Capture_marks_authorization_absent_when_header_missing()
    {
        DefaultHttpContext context = CreateContext(
            method: "POST",
            path: "/api/uploads/symbol",
            authorization: null);

        BasicAuthChallengeInfo info = BasicAuthChallengeDiagnostics.Capture(context);

        Assert.False(info.AuthorizationHeaderPresent);
        Assert.False(info.UsernamePresent);
    }

    [Fact]
    public void Capture_and_rendered_messages_never_include_authorization_material()
    {
        DefaultHttpContext context = CreateContext(
            method: "GET",
            path: "/download/symbols/ntdll.pdb/abc/ntdll.pdb",
            authorization: AuthorizationValue,
            userAgent: "Microsoft-Symbol-Server/10.0");

        BasicAuthChallengeInfo info = BasicAuthChallengeDiagnostics.Capture(context, usernamePresent: true);

        foreach (string? field in new[]
                 {
                     info.Method, info.Path, info.EndpointDisplayName, info.RemoteIp, info.UserAgent, info.TraceId
                 })
        {
            Assert.DoesNotContain(AuthorizationValue, field ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain(BasicPayload, field ?? string.Empty, StringComparison.Ordinal);
            Assert.DoesNotContain("user:password", field ?? string.Empty, StringComparison.Ordinal);
        }

        string challenge = BasicAuthChallengeDiagnostics.RenderChallengeMessage(info);
        string rejection = BasicAuthChallengeDiagnostics.RenderRejectionMessage(info);

        Assert.DoesNotContain(AuthorizationValue, challenge, StringComparison.Ordinal);
        Assert.DoesNotContain(AuthorizationValue, rejection, StringComparison.Ordinal);
        Assert.DoesNotContain(BasicPayload, challenge, StringComparison.Ordinal);
        Assert.DoesNotContain(BasicPayload, rejection, StringComparison.Ordinal);
        Assert.DoesNotContain("user:password", challenge, StringComparison.Ordinal);
        Assert.DoesNotContain("user:password", rejection, StringComparison.Ordinal);
        Assert.DoesNotContain("Authorization:", challenge, StringComparison.Ordinal);
        Assert.DoesNotContain("Authorization:", rejection, StringComparison.Ordinal);

        Assert.Contains("authorizationHeaderPresent=True", challenge, StringComparison.Ordinal);
        Assert.Contains("usernamePresent=True", rejection, StringComparison.Ordinal);
        Assert.Contains("/download/symbols/ntdll.pdb/abc/ntdll.pdb", challenge, StringComparison.Ordinal);
    }

    [Fact]
    public void HttpContext_overload_detects_basic_challenge_on_response()
    {
        DefaultHttpContext context = CreateContext("GET", "/auth-challenge");
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = "Basic realm=\"Basic Authentication\"";

        Assert.True(BasicAuthChallengeDiagnostics.IsBasicChallenge(context));
    }

    private static DefaultHttpContext CreateContext(
        string method,
        string path,
        string? authorization = null,
        string? userAgent = null,
        string? remoteIp = null,
        string? traceId = null,
        string? endpointDisplayName = null)
    {
        DefaultHttpContext context = new();
        context.Request.Method = method;
        context.Request.Path = path;
        context.TraceIdentifier = traceId ?? "test-trace";

        if (authorization is not null)
        {
            context.Request.Headers.Authorization = authorization;
        }

        if (userAgent is not null)
        {
            context.Request.Headers.UserAgent = userAgent;
        }

        if (remoteIp is not null)
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(remoteIp);
        }

        if (endpointDisplayName is not null)
        {
            context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, EndpointMetadataCollection.Empty, endpointDisplayName));
        }

        return context;
    }
}
