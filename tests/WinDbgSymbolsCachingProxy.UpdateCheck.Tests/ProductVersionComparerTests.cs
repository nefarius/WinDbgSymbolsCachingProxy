namespace WinDbgSymbolsCachingProxy.UpdateCheck.Tests;

public class ProductVersionComparerTests
{
    [Theory]
    [InlineData("1.31.2", "1.31.2.0")]
    [InlineData("1.31.2+sha", "1.31.2.0")]
    [InlineData("1.31.2-beta.1", "1.31.2.0")]
    [InlineData(" 1.31.2.0 ", "1.31.2.0")]
    public void NormalizesEquivalentProductVersions(string candidate, string baseline)
    {
        Assert.False(ProductVersionComparer.IsNewer(candidate, baseline));
        Assert.False(ProductVersionComparer.IsNewer(baseline, candidate));
    }

    [Fact]
    public void ParsesMissingPartsAsZero()
    {
        bool parsed = ProductVersionComparer.TryParse("1.31.2", out Version? version);

        Assert.True(parsed);
        Assert.Equal(new Version(1, 31, 2, 0), version);
    }

    [Theory]
    [InlineData("1.31.3.0", "1.31.2.9")]
    [InlineData("2.0.0.0", "1.31.2.0")]
    [InlineData("1.32.0-beta.1", "1.31.2.0")]
    public void DetectsNewerNumericVersion(string candidate, string baseline)
    {
        Assert.True(ProductVersionComparer.IsNewer(candidate, baseline));
    }

    [Theory]
    [InlineData("1.31.2.0", "1.32.0.0")]
    [InlineData("nope", "1.0.0.0")]
    [InlineData("1.0.0.0", "nope")]
    [InlineData("1.2.3.4.5", "1.0.0.0")]
    [InlineData(null, null)]
    public void DoesNotTreatOlderOrUnparsableValuesAsNewer(string? candidate, string? baseline)
    {
        Assert.False(ProductVersionComparer.IsNewer(candidate, baseline));
    }
}
