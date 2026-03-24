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
            return loaded ?? AgentSettingsDocument.CreateDefault();
        }
        catch
        {
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
            _document = Clone(document);
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
}
