using System.Net.Http.Headers;
using System.Text;

using Microsoft.Extensions.Options;

using MimeTypes;

namespace HarvestingAgent;

public class HarvestingBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HarvestingBackgroundService> _logger;
    private readonly IReadOnlyDictionary<FileSystemWatcher, ServerConfig> _maps;

    public HarvestingBackgroundService(ILogger<HarvestingBackgroundService> logger, IOptions<ServiceConfig> config,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        Dictionary<FileSystemWatcher, ServerConfig> maps = new();

        foreach (ServerConfig serverConfig in config.Value.Servers)
        {
            foreach (FileSystemWatcher watcher in serverConfig.WatcherPaths.Select(path => new FileSystemWatcher(path)
                     {
                         NotifyFilter = NotifyFilters.Attributes
                                        | NotifyFilters.CreationTime
                                        | NotifyFilters.DirectoryName
                                        | NotifyFilters.FileName
                                        | NotifyFilters.LastWrite
                                        | NotifyFilters.Size
                     }))
            {
                // only watch those supported by the upload endpoint
                watcher.Filters.Add("*.exe");
                watcher.Filters.Add("*.dll");
                watcher.Filters.Add("*.sys");
                watcher.Filters.Add("*.pdb");

                logger.LogInformation("Watching over path {Path} ({@Filters})", watcher.Path, watcher.Filters);

                maps.Add(watcher, serverConfig);
            }
        }

        _maps = maps;
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

        foreach (KeyValuePair<FileSystemWatcher, ServerConfig> map in _maps)
        {
            map.Key.Created += WatcherOnCreated;
            map.Key.EnableRaisingEvents = true;
        }

        _logger.LogInformation("Watchers started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }

        foreach (KeyValuePair<FileSystemWatcher, ServerConfig> map in _maps)
        {
            map.Key.Created -= WatcherOnCreated;
            map.Key.EnableRaisingEvents = false;
        }

        _logger.LogInformation("Watchers stopped");

        return;

        void WatcherOnCreated(object sender, FileSystemEventArgs e)
        {
            FileSystemWatcher? watcher = sender as FileSystemWatcher;

            ArgumentNullException.ThrowIfNull(watcher);

            Task.Run(async () =>
            {
                try
                {
                    ServerConfig serverConfig = _maps[watcher];
                    HttpClient client = _httpClientFactory.CreateClient();

                    client.BaseAddress = serverConfig.ServerUrl;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                        "Basic",
                        Convert.ToBase64String(
                            Encoding.UTF8.GetBytes(
                                $"{serverConfig.Authentication.Username}:{serverConfig.Authentication.Password}")));

                    using MultipartFormDataContent form = new();
                    string path = e.FullPath;

                    // this event may fire while the content is still written to and locked
                    WaitForFile(path);

                    ByteArrayContent fileContent = new(await File.ReadAllBytesAsync(path, cts.Token));
                    string? mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(path));

                    ArgumentException.ThrowIfNullOrEmpty(mimeType);

                    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
                    form.Add(fileContent, "symbol", Path.GetFileName(path));

                    HttpResponseMessage response = await client.PostAsync(
                        "/api/uploads/symbol?force=true", // force flag overwrites any preexisting 404s
                        form,
                        cts.Token
                    );

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation("Symbol upload successful");

                        if (serverConfig.DeleteAfterUpload)
                        {
                            File.Delete(path);
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Symbol upload failed");
                        await File.WriteAllTextAsync($"{path}.upload-error.txt",
                            await response.Content.ReadAsStringAsync(cts.Token), cts.Token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to upload symbol {FullPath}", e.FullPath);
                }
            }, cts.Token);
        }
    }
}