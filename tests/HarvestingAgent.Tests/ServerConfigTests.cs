using HarvestingAgent;

namespace HarvestingAgent.Tests;

public class ServerConfigTests
{
    [Fact]
    public void DistinctTreatsEquivalentServerConfigsAsEqual()
    {
        ServerConfig first = CreateServerConfig();
        ServerConfig second = CreateServerConfig();

        ServerConfig[] distinct = [.. new[] { first, second }.Distinct()];

        Assert.Single(distinct);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    private static ServerConfig CreateServerConfig()
    {
        return new ServerConfig
        {
            ServerUrl = new Uri("https://symbols.example.test"),
            DisplayName = "Private Symbols",
            Authentication = new AuthenticationConfig
            {
                Username = "agent",
                Password = "secret"
            }
        };
    }
}
