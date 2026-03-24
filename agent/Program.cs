using System.Runtime.InteropServices;

using HarvestingAgent;
using HarvestingAgent.Components;

using MudBlazor.Services;

using Polly;
using Polly.Contrib.WaitAndRetry;

using Serilog;

WebApplicationOptions opts = new() { Args = args, ContentRootPath = AppContext.BaseDirectory };

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

string settingsPath = AgentSettingsPaths.SettingsFilePath;
AgentSettingsDocument initialSettings = AgentSettingsStore.LoadOrCreate(settingsPath);

WebApplicationBuilder builder = WebApplication.CreateBuilder(opts);

builder.Host.UseSerilog((context, _, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

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
builder.Services.AddHostedService<HarvestingHostedService>();

builder.Services.AddHttpClient("Server")
    .AddTransientHttpErrorPolicy(pb =>
        pb.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5)));

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
