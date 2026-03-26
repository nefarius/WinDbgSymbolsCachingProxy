using System.Text.Json;
using System.Text.Json.Serialization;

namespace HarvestingAgent;

public sealed class AgentSettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly object _lock = new();
    private readonly string _filePath;
    private AgentSettingsDocument _document;

    public AgentSettingsStore(string filePath, AgentSettingsDocument document)
    {
        _filePath = filePath;
        _document = document;
    }

    /// <summary>
    ///     Loads existing settings or creates defaults and writes the file if missing.
    /// </summary>
    public static AgentSettingsDocument LoadOrCreate(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        if (!File.Exists(filePath))
        {
            AgentSettingsDocument created = AgentSettingsDocument.CreateDefault();
            SaveToPath(filePath, created);
            return created;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            AgentSettingsDocument? loaded = JsonSerializer.Deserialize<AgentSettingsDocument>(json, JsonOptions);
            AgentSettingsDocument doc = loaded ?? AgentSettingsDocument.CreateDefault();
            bool migrated = ApplyMigrations(doc);
            if (migrated)
            {
                SaveToPath(filePath, doc);
            }

            return doc;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(
                $"[HarvestingAgent] Failed to load settings from {filePath}: {ex.GetType().Name}: {ex.Message}");
            return AgentSettingsDocument.CreateDefault();
        }
    }

    public AgentSettingsDocument GetSnapshot()
    {
        lock (_lock)
        {
            return Clone(_document);
        }
    }

    public void Replace(AgentSettingsDocument document)
    {
        lock (_lock)
        {
            AgentSettingsDocument cloned = Clone(document);
            ApplyMigrations(cloned);
            _document = cloned;
            SaveToPath(_filePath, _document);
        }
    }

    public void UpdateHarvestingEnabled(bool enabled)
    {
        lock (_lock)
        {
            _document.HarvestingEnabled = enabled;
            SaveToPath(_filePath, _document);
        }
    }

    public static void SaveToPath(string filePath, AgentSettingsDocument document)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        string json = JsonSerializer.Serialize(document, JsonOptions);
        string tempPath = Path.Combine(
            Path.GetDirectoryName(filePath)!,
            $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");

        try
        {
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }

    private static AgentSettingsDocument Clone(AgentSettingsDocument source)
    {
        string json = JsonSerializer.Serialize(source, JsonOptions);
        return JsonSerializer.Deserialize<AgentSettingsDocument>(json, JsonOptions)
               ?? AgentSettingsDocument.CreateDefault();
    }

    private static bool ApplyMigrations(AgentSettingsDocument doc)
    {
        bool changed = false;

        foreach (ServerConfig server in doc.Servers)
        {
            if (server.WatcherPaths.Count == 0)
            {
                continue;
            }

            // Migrate server-level upload filters into per-watched-path settings when unset.
            if (server.UploadFileFilters.Count > 0)
            {
                foreach (WatcherPathEntry watch in server.WatcherPaths)
                {
                    if (watch.UploadFileFilters.Count == 0)
                    {
                        watch.UploadFileFilters = server.UploadFileFilters.ToList();
                        changed = true;
                    }
                }
            }
        }

        return changed;
    }
}
