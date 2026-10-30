using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

using WinDbgSymbolsCachingProxy.Core;
using WinDbgSymbolsCachingProxy.Endpoints;
using WinDbgSymbolsCachingProxy.Validators;

namespace WinDbgSymbolsCachingProxy.Server.Tests;

public class HardeningTests
{
    [Theory]
    [InlineData("ntdll.pdb", true)]
    [InlineData("..", false)]
    [InlineData("a/../b", false)]
    [InlineData("a\\b", false)]
    [InlineData("a?b", false)]
    [InlineData("a#b", false)]
    [InlineData("a%2fb", false)]
    [InlineData("a\tb", false)]
    [InlineData("", false)]
    public void SafeSegment(string value, bool expected)
    {
        Assert.Equal(expected, SymbolsRequestValidator.IsSafeSegment(value));
    }

    [Theory]
    [InlineData("B96A69E1F3383F24ED7E10BBD2B3FFC81", true)]
    [InlineData("xyz", false)]
    [InlineData("", false)]
    public void HexKey(string value, bool expected)
    {
        Assert.Equal(expected, SymbolsRequestValidator.IsHex(value));
    }

    [Fact]
    public async Task MiddlewareReturns500ForUnhandledException()
    {
        DefaultHttpContext ctx = new();
        CustomExceptionHandlerMiddleware mw = new(
            NullLogger<CustomExceptionHandlerMiddleware>.Instance,
            _ => throw new InvalidOperationException("boom"));

        await mw.Invoke(ctx);

        Assert.Equal(500, ctx.Response.StatusCode);
    }

    [Fact]
    public void CrossSiteUploadIsRejectedWithoutApiKey()
    {
        DefaultHttpContext ctx = new();
        ctx.Request.Headers["Sec-Fetch-Site"] = "cross-site";
        Assert.True(SymbolUploadEndpoint.IsCrossSiteBrowserRequest(ctx.Request));

        ctx.Request.Headers["X-Api-Key"] = "k";
        Assert.False(SymbolUploadEndpoint.IsCrossSiteBrowserRequest(ctx.Request));
    }

    [Fact]
    public void BasicCredentialsCompareByValue()
    {
        BasicAuthCredentials a = new() { Username = "u", Password = "p" };
        Assert.True(a.Equals(new BasicAuthCredentials { Username = "u", Password = "p" }));
        Assert.False(a.Equals(new BasicAuthCredentials { Username = "u", Password = "x" }));
    }
}
