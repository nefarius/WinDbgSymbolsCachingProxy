using System.Text.Json;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

namespace HarvestingAgent;

[UsedImplicitly]
public sealed class WatcherPathEntry
{
    public string? Path { get; set; }

    /// <summary>
    ///     When true, <see cref="FileSystemWatcher.IncludeSubdirectories" /> is enabled for this path.
    /// </summary>
    public bool IncludeSubdirectories { get; set; }

    /// <summary>
    ///     File name patterns to monitor for this watched path (e.g. *.pdb). When empty, defaults to the runtime defaults.
    /// </summary>
    public List<string> UploadFileFilters { get; set; } = [];

    /// <summary>
    ///     When true, a successfully uploaded file may be deleted from disk (after all uploads succeed) if deletion rules match.
    /// </summary>
    public bool DeleteAfterUpload { get; set; } = false;

    /// <summary>
    ///     Inclusion filter for deletion matching when <see cref="DeleteAfterUpload" /> is true. Accepts filesystem globbing patterns.
    /// </summary>
    /// <remarks>See https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing</remarks>
    public List<string> DeletionInclusionFilter { get; set; } = ["**/*.pdb"];

    /// <summary>
    ///     Exclusion filter for deletion matching when <see cref="DeleteAfterUpload" /> is true. Accepts filesystem globbing patterns.
    /// </summary>
    /// <remarks>See https://learn.microsoft.com/en-us/dotnet/core/extensions/file-globbing</remarks>
    public List<string> DeletionExclusionFilter { get; set; } = [];

    /// <summary>
    ///     Migrates legacy settings where a trailing <c>*</c> denoted recursive watching.
    /// </summary>
    public static WatcherPathEntry FromLegacyPathString(string? raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return new WatcherPathEntry();
        }

        if (raw.EndsWith('*'))
        {
            return new WatcherPathEntry
            {
                Path = raw.TrimEnd('*'),
                IncludeSubdirectories = true
            };
        }

        return new WatcherPathEntry { Path = raw, IncludeSubdirectories = false };
    }
}

/// <summary>
///     Supports current JSON objects <c>[{ "path", "includeSubdirectories" }]</c> and legacy string arrays
///     <c>["D:\\a", "D:\\b*"]</c>.
/// </summary>
public sealed class WatcherPathEntryListJsonConverter : JsonConverter<List<WatcherPathEntry>>
{
    public override List<WatcherPathEntry> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected start of array for watcherPaths.");
        }

        List<WatcherPathEntry> list = [];

        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    list.Add(WatcherPathEntry.FromLegacyPathString(reader.GetString()));
                    break;
                case JsonTokenType.StartObject:
                    WatcherPathEntry? entry = JsonSerializer.Deserialize<WatcherPathEntry>(ref reader, options);
                    if (entry is not null)
                    {
                        list.Add(NormalizeLegacyTrailingStar(entry));
                    }

                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, List<WatcherPathEntry> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (WatcherPathEntry e in value)
        {
            writer.WriteStartObject();
            writer.WriteString("path", e.Path);
            writer.WriteBoolean("includeSubdirectories", e.IncludeSubdirectories);
            writer.WritePropertyName("uploadFileFilters");
            JsonSerializer.Serialize(writer, e.UploadFileFilters, options);
            writer.WriteBoolean("deleteAfterUpload", e.DeleteAfterUpload);
            writer.WritePropertyName("deletionInclusionFilter");
            JsonSerializer.Serialize(writer, e.DeletionInclusionFilter, options);
            writer.WritePropertyName("deletionExclusionFilter");
            JsonSerializer.Serialize(writer, e.DeletionExclusionFilter, options);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    /// <summary>
    ///     If JSON was hand-edited with a trailing star on <see cref="WatcherPathEntry.Path" />, fold it into the flag.
    /// </summary>
    private static WatcherPathEntry NormalizeLegacyTrailingStar(WatcherPathEntry entry)
    {
        string? p = entry.Path;
        if (string.IsNullOrEmpty(p))
        {
            return entry;
        }

        if (p.EndsWith('*'))
        {
            return new WatcherPathEntry
            {
                Path = p.TrimEnd('*'),
                IncludeSubdirectories = true,
                UploadFileFilters = [.. entry.UploadFileFilters],
                DeleteAfterUpload = entry.DeleteAfterUpload,
                DeletionInclusionFilter = [.. entry.DeletionInclusionFilter],
                DeletionExclusionFilter = [.. entry.DeletionExclusionFilter]
            };
        }

        return entry;
    }
}
