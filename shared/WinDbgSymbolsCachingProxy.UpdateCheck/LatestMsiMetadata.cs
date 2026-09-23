using System.Text.Json.Serialization;

namespace WinDbgSymbolsCachingProxy.UpdateCheck;

/// <summary>
///     Metadata published beside the latest MSI artifact.
/// </summary>
public sealed class LatestMsiMetadata
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("ProductVersion")]
    public string? ProductVersion { get; set; }

    [JsonPropertyName("ProductName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("Manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonPropertyName("ProductCode")]
    public string? ProductCode { get; set; }
}
