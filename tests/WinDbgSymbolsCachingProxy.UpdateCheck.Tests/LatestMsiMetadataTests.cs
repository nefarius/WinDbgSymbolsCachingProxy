using System.Text.Json;

namespace WinDbgSymbolsCachingProxy.UpdateCheck.Tests;

public class LatestMsiMetadataTests
{
    [Fact]
    public void DeserializesBuildbotMetadata()
    {
        const string json = """
            {
              "type": "msi",
              "ProductVersion": "1.31.2.0",
              "ProductName": "WinDbg Symbols Caching Proxy",
              "Manufacturer": "Nefarius Software Solutions",
              "ProductCode": "{A4F8B2C1-3D5E-4A7B-8C9D-0E1F3B2B6C5E}"
            }
            """;

        LatestMsiMetadata? metadata = JsonSerializer.Deserialize<LatestMsiMetadata>(json);

        Assert.NotNull(metadata);
        Assert.Equal("msi", metadata.Type);
        Assert.Equal("1.31.2.0", metadata.ProductVersion);
        Assert.Equal("WinDbg Symbols Caching Proxy", metadata.ProductName);
        Assert.Equal("Nefarius Software Solutions", metadata.Manufacturer);
        Assert.Equal("{A4F8B2C1-3D5E-4A7B-8C9D-0E1F3B2B6C5E}", metadata.ProductCode);
    }
}
