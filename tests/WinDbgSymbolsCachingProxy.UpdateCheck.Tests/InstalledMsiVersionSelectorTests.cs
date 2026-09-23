namespace WinDbgSymbolsCachingProxy.UpdateCheck.Tests;

public class InstalledMsiVersionSelectorTests
{
    [Fact]
    public void SelectsMatchingMsiDisplayVersion()
    {
        InstalledProductRecord[] products =
        [
            new("Other Product", InstalledMsiVersionSelector.Manufacturer, "9.9.9.9", true),
            new(
                "windbg symbols caching proxy",
                "nefarius software solutions",
                " 1.31.2.0 ",
                true),
            new(InstalledMsiVersionSelector.ProductName, InstalledMsiVersionSelector.Manufacturer, "2.0.0.0", true)
        ];

        string? version = InstalledMsiVersionSelector.Select(products);

        Assert.Equal("1.31.2.0", version);
    }

    [Theory]
    [InlineData(false)]
    public void IgnoresEntriesThatAreNotTheProductMsi(bool isWindowsInstaller)
    {
        InstalledProductRecord[] products =
        [
            new(
                InstalledMsiVersionSelector.ProductName,
                InstalledMsiVersionSelector.Manufacturer,
                "1.31.2.0",
                isWindowsInstaller),
            new(InstalledMsiVersionSelector.ProductName, "Another Publisher", "1.31.2.0", true)
        ];

        Assert.Null(InstalledMsiVersionSelector.Select(products));
    }
}
