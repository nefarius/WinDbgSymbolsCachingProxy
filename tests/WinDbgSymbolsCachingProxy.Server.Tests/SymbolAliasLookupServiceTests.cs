using WinDbgSymbolsCachingProxy.Models;
using WinDbgSymbolsCachingProxy.Services;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public sealed class SymbolAliasLookupServiceTests
{
    [Theory]
    [InlineData("foo.sys/abc123/", "foo.sys")]
    [InlineData("BAR.PDB/guid/", "bar.pdb")]
    public void TryParseSymbolFromIndexPrefix_returns_first_segment_lower(string indexPrefix, string expected)
    {
        Assert.True(SymbolAliasLookupService.TryParseSymbolFromIndexPrefix(indexPrefix, out string seg));
        Assert.Equal(expected, seg);
    }

    [Theory]
    [InlineData("")]
    [InlineData("noseparator")]
    [InlineData("/onlylead")]
    public void TryParseSymbolFromIndexPrefix_returns_false_when_invalid(string indexPrefix)
    {
        Assert.False(SymbolAliasLookupService.TryParseSymbolFromIndexPrefix(indexPrefix, out _));
    }

    [Fact]
    public void PickBest_prefers_most_recent_UploadedAt()
    {
        DateTime older = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        DateTime newer = new(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        SymbolsEntity a = new()
        {
            IndexPrefix = "a/",
            FileName = "a",
            SymbolKey = "k",
            UploadedAt = older
        };

        SymbolsEntity b = new()
        {
            IndexPrefix = "b/",
            FileName = "b",
            SymbolKey = "k",
            UploadedAt = newer
        };

        SymbolsEntity? best = SymbolAliasLookupService.PickBest([a, b]);
        Assert.Same(b, best);
    }

    [Fact]
    public void PickBest_single_returns_that_item()
    {
        SymbolsEntity only = new() { IndexPrefix = "x/", FileName = "x", SymbolKey = "k" };
        Assert.Same(only, SymbolAliasLookupService.PickBest([only]));
    }

    [Fact]
    public void PickBest_empty_returns_null()
    {
        Assert.Null(SymbolAliasLookupService.PickBest(Array.Empty<SymbolsEntity>()));
    }
}
