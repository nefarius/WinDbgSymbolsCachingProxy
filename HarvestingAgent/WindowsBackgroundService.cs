using System.Net.Http.Headers;

using Microsoft.Extensions.Options;

using MimeTypes;

namespace HarvestingAgent;

public class WindowsBackgroundService : BackgroundService
{
    private readonly IOptions<ServiceConfig> _config;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WindowsBackgroundService> _logger;
    private readonly FileSystemWatcher _watcher;

    public WindowsBackgroundService(ILogger<WindowsBackgroundService> logger, IOptions<ServiceConfig> config,
        FileSystemWatcher watcher, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _config = config;
        _watcher = watcher;
        _httpClientFactory = httpClientFactory;
    }

    private void WaitForFile(string fullPath)
    {
        while (true)
        {
            try
            {
                using StreamReader stream = new(fullPath);
                break;
            }
            catch
            {
                Thread.Sleep(1000);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        _watcher.Created += WatcherOnCreated;

        _watcher.EnableRaisingEvents = true;
        
        _logger.LogInformation("Watcher started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        _watcher.EnableRaisingEvents = false;
        
        _logger.LogInformation("Watcher stopped");
        
        return;

        void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            Task.Run(async () =>
            {
                try
                {
                    HttpClient client = _httpClientFactory.CreateClient("Server");
                    using MultipartFormDataContent form = new();
                    string path = e.FullPath;

                    // this event may fire while the content is still written to and locked
                    WaitForFile(path);

                    ByteArrayContent fileContent = new(await File.ReadAllBytesAsync(path, cts.Token));
                    string? mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                    ArgumentException.ThrowIfNullOrEmpty(mimeType);

                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                    form.Add(fileContent, "symbol", Path.GetFileName(path));

                    HttpResponseMessage response = await client.PostAsync("/api/uploads/symbol", form, cts.Token);

                    _logger.LogInformation(response.IsSuccessStatusCode
                        ? "Symbol upload successful"
                        : "Symbol upload failed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload symbol {FullPath}", e.FullPath);
                }
            }, cts.Token);
        }
    }
}