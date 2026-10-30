using System.Security.Claims;

namespace HarvestingAgent.Tests;

public sealed class AgentHardeningTests : IDisposable
{
    private readonly string _directory = Directory.CreateTempSubdirectory("agent-settings-").FullName;

    public void Dispose()
    {
        Directory.Delete(_directory, true);
    }

    private static ClaimsPrincipal PrincipalWith(string claimType, string sid)
    {
        return new ClaimsPrincipal(new ClaimsIdentity([new Claim(claimType, sid)], "Negotiate"));
    }

    [Theory]
    [InlineData(ClaimTypes.GroupSid, "S-1-5-32-544", true)]
    [InlineData(ClaimTypes.DenyOnlySid, "S-1-5-32-544", true)] // non-elevated administrator
    [InlineData(ClaimTypes.GroupSid, "S-1-5-32-545", false)] // Users
    [InlineData(ClaimTypes.Role, "S-1-5-32-544", false)]
    public void LocalAdministratorCheck_RecognizesAdministratorsBySid(string claimType, string sid, bool expected)
    {
        Assert.Equal(expected, LocalAdministratorCheck.IsLocalAdministrator(PrincipalWith(claimType, sid)));
    }

    [Fact]
    public void LoadOrCreate_KeepsCorruptSettingsAsBackup()
    {
        string path = Path.Combine(_directory, "agent-settings.json");
        File.WriteAllText(path, "{ not json");

        AgentSettingsDocument doc = AgentSettingsStore.LoadOrCreate(path);

        Assert.Empty(doc.Servers);
        Assert.Single(Directory.GetFiles(_directory, "agent-settings.json.corrupt-*"));
    }

    [Fact]
    public void ReplaceServers_DoesNotOverwriteHarvestingToggle()
    {
        string path = Path.Combine(_directory, "agent-settings.json");
        AgentSettingsDocument initial = AgentSettingsStore.LoadOrCreate(path);
        AgentSettingsStore store = new(path, initial);

        // The configuration page took its snapshot while harvesting was still enabled ...
        AgentSettingsDocument staleSnapshot = store.GetSnapshot();
        staleSnapshot.Servers.Add(new ServerConfig { DisplayName = "new", ApiKey = "key" });

        // ... and the user disables harvesting from the dashboard before saving.
        store.UpdateHarvestingEnabled(false);
        store.ReplaceServers(staleSnapshot.Servers);

        AgentSettingsDocument reloaded = AgentSettingsStore.LoadOrCreate(path);
        Assert.False(reloaded.HarvestingEnabled);
        Assert.Equal("new", Assert.Single(reloaded.Servers).DisplayName);
        Assert.Equal("key", reloaded.Servers[0].ApiKey);
    }

    [Fact]
    public void SaveToPath_LeavesNoTemporaryFiles()
    {
        string path = Path.Combine(_directory, "agent-settings.json");

        AgentSettingsStore.SaveToPath(path, AgentSettingsDocument.CreateDefault());

        Assert.Equal([path], Directory.GetFiles(_directory));
    }
}
