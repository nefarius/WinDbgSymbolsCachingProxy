using System.Text;
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
        string settingsDirectory = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(settingsDirectory);
        AgentSettingsPaths.RestrictSettingsDirectory(settingsDirectory);

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

            // Keep the unreadable file so the next save does not silently destroy the user's configuration.
            try
            {
                string backup = $"{filePath}.corrupt-{DateTime.UtcNow:yyyyMMddHHmmss}";
                // The file holds credentials: create the backup owner-only from the start (File.Copy would inherit the
                // source mode, and a umask-dependent mode would briefly expose it). Never overwrites an existing backup.
                FileStreamOptions backupOptions = new()
                {
                    Mode = FileMode.CreateNew,
                    Access = FileAccess.Write,
                    Share = FileShare.None
                };
                if (!OperatingSystem.IsWindows())
                {
                    backupOptions.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                }

                using (FileStream source = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (FileStream destination = new(backup, backupOptions))
                {
                    source.CopyTo(destination);
                }

                Console.Error.WriteLine($"[HarvestingAgent] Preserved the unreadable settings file as {backup}");
            }
            catch (Exception backupEx)
            {
                Console.Error.WriteLine($"[HarvestingAgent] Could not back up the unreadable settings file: {backupEx.Message}");
            }

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

    /// <summary>
    ///     Replaces only the configured servers, leaving other settings (such as the harvesting toggle) untouched so a stale
    ///     configuration page cannot overwrite concurrent changes.
    /// </summary>
    public void ReplaceServers(IEnumerable<ServerConfig> servers)
    {
        AgentSettingsDocument incoming = new() { Servers = servers.ToList() };
        AgentSettingsDocument migrated = Clone(incoming);
        ApplyMigrations(migrated);

        lock (_lock)
        {
            // Persist a candidate first so a failed save leaves the in-memory document unchanged.
            AgentSettingsDocument candidate = Clone(_document);
            candidate.Servers = migrated.Servers;
            SaveToPath(_filePath, candidate);
            _document = candidate;
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
            // Flush to disk before the rename so a power loss cannot leave a truncated settings file.
            using (FileStream stream = new(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                byte[] bytes = new UTF8Encoding(false).GetBytes(json);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }

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
