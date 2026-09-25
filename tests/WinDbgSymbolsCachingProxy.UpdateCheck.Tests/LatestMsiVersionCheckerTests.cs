using System.Net;
using System.Text;

using Microsoft.Extensions.DependencyInjection;

namespace WinDbgSymbolsCachingProxy.UpdateCheck.Tests;

public class LatestMsiVersionCheckerTests
{
    [Fact]
    public async Task PrefersInstalledMsiOverAssemblyVersion()
    {
        RecordingHandler handler = new(Json("1.1.0.0"));
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: "1.0.0.0", assembly: "9.0.0.0");

        VersionCheckStatus status = await checker.GetStatusAsync();

        Assert.Equal("1.0.0.0", status.LocalVersion);
        Assert.Equal(LocalVersionOrigin.InstalledMsi, status.Origin);
        Assert.Equal("1.1.0.0", status.LatestVersion);
        Assert.True(status.UpdateAvailable);
        Assert.Equal(LatestMsiVersionChecker.DownloadUri, status.DownloadUri);
        Assert.Equal("1.0.0.0 · 1.1.0.0 available", status.ChipLabel);
        Assert.Contains("Installed MSI version 1.0.0.0", status.Tooltip, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FallsBackToAssemblyWhenMsiIsMissing()
    {
        RecordingHandler handler = new(Json("1.0.1.0"));
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: null, assembly: "1.0.0+sha");

        VersionCheckStatus status = await checker.GetStatusAsync();

        Assert.Equal("1.0.0+sha", status.LocalVersion);
        Assert.Equal(LocalVersionOrigin.Assembly, status.Origin);
        Assert.True(status.UpdateAvailable);
        Assert.Contains("Running version", status.Tooltip, StringComparison.Ordinal);
    }

    [Fact]
    public async Task FallsBackToAssemblyWhenInstalledVersionCannotBeParsed()
    {
        RecordingHandler handler = new(Json("1.2.0.0"));
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: "not-a-version", assembly: "1.0.0.0");

        VersionCheckStatus status = await checker.GetStatusAsync();

        Assert.Equal("1.0.0.0", status.LocalVersion);
        Assert.Equal(LocalVersionOrigin.Assembly, status.Origin);
        Assert.True(status.UpdateAvailable);
    }

    [Fact]
    public async Task FallsBackToAssemblyWhenRegistryLookupThrows()
    {
        RecordingHandler handler = new(Json("1.2.0.0"));
        LatestMsiVersionChecker checker = CreateChecker(
            handler,
            new ThrowingInstalledMsiVersionSource(),
            new FixedAssemblyVersionSource("1.0.0.0"));

        VersionCheckStatus status = await checker.GetStatusAsync();

        Assert.Equal(LocalVersionOrigin.Assembly, status.Origin);
        Assert.True(status.UpdateAvailable);
    }

    [Theory]
    [InlineData("1.0.0.0")]
    [InlineData("0.9.0.0")]
    public async Task DoesNotOfferDownloadWhenLatestIsNotNewer(string latest)
    {
        RecordingHandler handler = new(Json(latest));
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: "1.0.0.0", assembly: "9.9.9.9");

        VersionCheckStatus status = await checker.GetStatusAsync();

        Assert.False(status.UpdateAvailable);
        Assert.Null(status.DownloadUri);
        Assert.Equal("1.0.0.0", status.ChipLabel);
    }

    [Fact]
    public async Task CachesMetadataUntilLifetimeExpires()
    {
        ManualTimeProvider time = new();
        RecordingHandler handler = new(Json("2.0.0.0"));
        LatestMsiVersionChecker checker = CreateChecker(
            handler,
            installed: "1.0.0.0",
            assembly: null,
            time,
            TimeSpan.FromMinutes(15));

        VersionCheckStatus first = await checker.GetStatusAsync();
        VersionCheckStatus second = await checker.GetStatusAsync();
        Assert.Equal(1, handler.Calls);

        time.UtcNow += TimeSpan.FromMinutes(15);
        handler.ResponseJson = Json("3.0.0.0");
        VersionCheckStatus third = await checker.GetStatusAsync();

        Assert.Equal(2, handler.Calls);
        Assert.Equal(first.LatestVersion, second.LatestVersion);
        Assert.Equal("3.0.0.0", third.LatestVersion);
        Assert.Equal(LatestMsiVersionChecker.MetadataUri, handler.LastRequestUri);
    }

    [Theory]
    [InlineData(HttpStatusCode.ServiceUnavailable, "{\"ProductVersion\":\"9.0.0.0\"}")]
    [InlineData(HttpStatusCode.OK, "not-json")]
    [InlineData(HttpStatusCode.OK, "{\"type\":\"zip\",\"ProductVersion\":\"9.0.0.0\"}")]
    public async Task KeepsLocalVersionWhenMetadataCannotBeUsed(HttpStatusCode statusCode, string body)
    {
        RecordingHandler handler = new(body, statusCode);
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: "1.0.0.0", assembly: null);

        VersionCheckStatus status = await checker.GetStatusAsync();
        VersionCheckStatus cached = await checker.GetStatusAsync();

        Assert.Equal("1.0.0.0", status.LocalVersion);
        Assert.False(status.UpdateAvailable);
        Assert.Null(status.LatestVersion);
        Assert.Equal(1, handler.Calls);
        Assert.False(cached.UpdateAvailable);
    }

    [Fact]
    public async Task KeepsLocalVersionWhenFetchThrows()
    {
        RecordingHandler handler = new(_ => throw new HttpRequestException("offline"));
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: null, assembly: "1.4.0");

        VersionCheckStatus status = await checker.GetStatusAsync();

        Assert.Equal("1.4.0", status.LocalVersion);
        Assert.False(status.UpdateAvailable);
    }

    [Fact]
    public void LocalStatusDoesNotRequireMetadata()
    {
        RecordingHandler handler = new(_ => throw new HttpRequestException("offline"));
        LatestMsiVersionChecker checker = CreateChecker(handler, installed: "1.5.0.0", assembly: "9.0.0.0");

        VersionCheckStatus status = checker.GetLocalStatus();

        Assert.Equal("1.5.0.0", status.LocalVersion);
        Assert.Equal(0, handler.Calls);
        Assert.False(status.UpdateAvailable);
    }

    [Fact]
    public void RegistersCheckerAsSingleton()
    {
        ServiceCollection services = new();
        services.AddSingleton<ILocalAssemblyVersionSource>(new FixedAssemblyVersionSource("1.0.0.0"));
        services.AddLatestMsiVersionCheck();

        using ServiceProvider provider = services.BuildServiceProvider();
        LatestMsiVersionChecker first = provider.GetRequiredService<LatestMsiVersionChecker>();
        LatestMsiVersionChecker second = provider.GetRequiredService<LatestMsiVersionChecker>();

        Assert.Same(first, second);
    }

    private static LatestMsiVersionChecker CreateChecker(
        RecordingHandler handler,
        string? installed,
        string? assembly,
        TimeProvider? timeProvider = null,
        TimeSpan? cacheLifetime = null)
    {
        return CreateChecker(
            handler,
            new FixedInstalledMsiVersionSource(installed),
            new FixedAssemblyVersionSource(assembly),
            timeProvider,
            cacheLifetime);
    }

    private static LatestMsiVersionChecker CreateChecker(
        RecordingHandler handler,
        IInstalledMsiVersionSource installed,
        ILocalAssemblyVersionSource assembly,
        TimeProvider? timeProvider = null,
        TimeSpan? cacheLifetime = null)
    {
        HttpClient client = new(handler, disposeHandler: false);
        return new LatestMsiVersionChecker(client, installed, assembly, timeProvider, cacheLifetime);
    }

    private static string Json(string productVersion)
    {
        return $$"""
            {
              "type": "msi",
              "ProductVersion": "{{productVersion}}",
              "ProductName": "WinDbg Symbols Caching Proxy",
              "Manufacturer": "Nefarius Software Solutions",
              "ProductCode": "{A4F8B2C1-3D5E-4A7B-8C9D-0E1F3B2B6C5E}"
            }
            """;
    }

    private sealed class FixedInstalledMsiVersionSource(string? version) : IInstalledMsiVersionSource
    {
        public string? TryGetVersion()
        {
            return version;
        }
    }

    private sealed class ThrowingInstalledMsiVersionSource : IInstalledMsiVersionSource
    {
        public string? TryGetVersion()
        {
            throw new InvalidOperationException("registry unavailable");
        }
    }

    private sealed class FixedAssemblyVersionSource(string? version) : ILocalAssemblyVersionSource
    {
        public string? TryGetVersion()
        {
            return version;
        }
    }

    private sealed class ManualTimeProvider : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 9, 23, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow()
        {
            return UtcNow;
        }
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage>? _responder;
        private int _calls;

        public RecordingHandler(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            ResponseJson = body;
            StatusCode = statusCode;
        }

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        public string ResponseJson { get; set; } = "";

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        public int Calls => _calls;

        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _calls);
            LastRequestUri = request.RequestUri;
            if (_responder is not null)
                return Task.FromResult(_responder(request));

            HttpResponseMessage response = new(StatusCode)
            {
                Content = new StringContent(ResponseJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
