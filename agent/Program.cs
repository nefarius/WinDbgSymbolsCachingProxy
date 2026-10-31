using System.Runtime.InteropServices;

using HarvestingAgent;
using HarvestingAgent.Components;
using HarvestingAgent.Logging;

using WinDbgSymbolsCachingProxy.UpdateCheck;

using Microsoft.Extensions.Http.Resilience;

using MudBlazor.Services;

using Serilog;

WebApplicationOptions opts = new() { Args = args, ContentRootPath = AppContext.BaseDirectory };

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

string settingsPath = AgentSettingsPaths.SettingsFilePath;
AgentSettingsDocument initialSettings = AgentSettingsStore.LoadOrCreate(settingsPath);

WebApplicationBuilder builder = WebApplication.CreateBuilder(opts);
LogBufferService logBuffer = new();

builder.Host.UseSerilog((context, _, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Sink(new LogBufferSink(logBuffer)));

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Host.UseWindowsService(options =>
    {
        options.ServiceName = "Debug Symbols Harvesting Agent";
    });

    Log.Information("Configured as Windows Service: Debug Symbols Harvesting Agent");
}

builder.WebHost.UseUrls($"http://127.0.0.1:{initialSettings.ListenPort}");

builder.Services.AddSingleton(new AgentSettingsStore(settingsPath, initialSettings));
builder.Services.AddSingleton(new AgentStartupContext { ListenPortAtStartup = initialSettings.ListenPort });
builder.Services.AddSingleton<DirectoryBrowserService>();
builder.Services.AddSingleton<HarvesterHealthState>();
builder.Services.AddSingleton<HarvesterRuntime>();
builder.Services.AddSingleton(logBuffer);
builder.Services.AddHostedService<HarvestingHostedService>();

builder.Services.AddHttpClient("Server", client =>
        // Timeouts are enforced by the resilience pipeline below; the default 100 s would cut off large uploads.
        client.Timeout = Timeout.InfiniteTimeSpan)
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 4;
        options.Retry.Delay = TimeSpan.FromSeconds(1);
        options.Retry.UseJitter = true;
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(10);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(45);
        // sampling duration must be at least twice the attempt timeout
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(20);
    })
    // The base address is set per request, so one named client talks to several servers; give each
    // server authority its own circuit breaker instead of letting one failing server block the others.
    .SelectPipelineByAuthority();
builder.Services.AddSingleton<ILocalAssemblyVersionSource, AgentAssemblyVersionSource>();
builder.Services.AddLatestMsiVersionCheck();

builder.Services.AddMudServices();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
