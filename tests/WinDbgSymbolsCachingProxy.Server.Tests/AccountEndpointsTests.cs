using WinDbgSymbolsCachingProxy.Core;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public class AccountEndpointsTests
{
    [Theory]
    [InlineData("/", true)]
    [InlineData("/search?q=a", true)]
    [InlineData("//evil.example", false)]
    [InlineData("/\\evil.example", false)]
    [InlineData("/\t/evil.example", false)]
    [InlineData("https://evil.example", false)]
    [InlineData("evil.example", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsLocalUrl_OnlyAcceptsRelativeSameSitePaths(string? url, bool expected)
    {
        Assert.Equal(expected, AccountEndpoints.IsLocalUrl(url));
    }
}
