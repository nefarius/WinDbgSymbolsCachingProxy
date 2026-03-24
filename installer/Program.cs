using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using WixSharp;

namespace WinDbgSymbolsCachingProxy.Installer;

static class Program
{
    const string DefaultServerRelative = "publish-x64/server";
    const string DefaultAgentRelative = "publish-x64/agent";

    /// <summary>
    ///     NUKE passes these so paths are not collapsed into a single <c>dotnet run</c> application argument.
    /// </summary>
    internal const string EnvServer = "WDSCP_INSTALLER_SERVER";
    internal const string EnvAgent = "WDSCP_INSTALLER_AGENT";
    internal const string EnvOut = "WDSCP_INSTALLER_OUT";

    static int Main(string[] args)
    {
        try
        {
            var options = Options.Parse(args);
            options.Validate();

            string licensePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "License.rtf"));
            if (!System.IO.File.Exists(licensePath))
                throw new FileNotFoundException($"License file not found: {licensePath}");

            var featureServer = new Feature(
                "Server",
                "Symbols caching proxy: web UI, REST API, and symbol store (WinDbgSymbolsCachingProxy).");
            var featureAgent = new Feature(
                "Agent",
                "Workstation harvesting agent (HarvestingAgent).");

            string serverMask = Path.Combine(options.ServerPublishDir, "*.*");
            string agentMask = Path.Combine(options.AgentPublishDir, "*.*");

            var project = new Project(
                "WinDbg Symbols Caching Proxy",
                new Dir(
                    @"%ProgramFiles64Folder%\Nefarius\WinDbg Symbols Caching Proxy",
                    new Dir("Server", new Files(featureServer, serverMask)),
                    new Dir("Agent", new Files(featureAgent, agentMask))))
            {
                GUID = new Guid("a4f8b2c1-3d5e-4a7b-8c9d-0e1f2a3b4c5d"),
                UpgradeCode = new Guid("e7a3c8f2-1b4d-4c9e-9f6a-0d2e8b5c7a91"),
                Platform = Platform.x64,
                Scope = InstallScope.perMachine,
                UI = WUI.WixUI_FeatureTree,
                LicenceFile = licensePath,
                Version = ReadProductVersion(options.ServerPublishDir),
                OutDir = options.OutputDir,
                OutFileName = "WinDbgSymbolsCachingProxy"
            };

            string msiPath = project.BuildMsi();
            Console.WriteLine(msiPath);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    static Version ReadProductVersion(string serverPublishDir)
    {
        string exePath = Path.Combine(serverPublishDir, "WinDbgSymbolsCachingProxy.exe");
        if (!System.IO.File.Exists(exePath))
            return new Version(1, 0, 0);

        var info = FileVersionInfo.GetVersionInfo(exePath);
        string? raw = info.ProductVersion ?? info.FileVersion;
        return TryParseVersion(raw, out Version? v) ? v : new Version(1, 0, 0);
    }

    static bool TryParseVersion(string? raw, [NotNullWhen(true)] out Version? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        string numeric = raw.Split('+', 2)[0].Split('-', 2)[0].Trim();
        var parts = numeric.Split('.');
        try
        {
            int major = parts.Length > 0 ? int.Parse(parts[0], CultureInfo.InvariantCulture) : 0;
            int minor = parts.Length > 1 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
            int build = parts.Length > 2 ? int.Parse(parts[2], CultureInfo.InvariantCulture) : 0;
            int revision = parts.Length > 3 ? int.Parse(parts[3], CultureInfo.InvariantCulture) : 0;
            version = new Version(major, minor, build, revision);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    sealed class Options
    {
        public required string ServerPublishDir { get; init; }
        public required string AgentPublishDir { get; init; }
        public required string OutputDir { get; init; }

        public static Options Parse(string[] args)
        {
            string cwd = Directory.GetCurrentDirectory();
            string server = Path.GetFullPath(Path.Combine(cwd, DefaultServerRelative));
            string agent = Path.GetFullPath(Path.Combine(cwd, DefaultAgentRelative));
            string output = Path.GetFullPath(Path.Combine(cwd, "publish-x64", "installer"));

            ApplyEnvironmentOverrides(ref server, ref agent, ref output);

            for (var i = 0; i < args.Length; i++)
            {
                string a = args[i];
                if (a is "--server" or "-s")
                    server = RequirePath(args, ref i, "server");
                else if (a is "--agent" or "-a")
                    agent = RequirePath(args, ref i, "agent");
                else if (a is "--out" or "-o")
                    output = RequirePath(args, ref i, "out");
                else if (a is "--help" or "-h")
                    PrintHelp();
                else
                    throw new ArgumentException($"Unknown argument: {a}");
            }

            return new Options
            {
                ServerPublishDir = server,
                AgentPublishDir = agent,
                OutputDir = output
            };
        }

        static void ApplyEnvironmentOverrides(ref string server, ref string agent, ref string output)
        {
            string? v = Environment.GetEnvironmentVariable(Program.EnvServer);
            if (!string.IsNullOrWhiteSpace(v))
                server = Path.GetFullPath(v);

            v = Environment.GetEnvironmentVariable(Program.EnvAgent);
            if (!string.IsNullOrWhiteSpace(v))
                agent = Path.GetFullPath(v);

            v = Environment.GetEnvironmentVariable(Program.EnvOut);
            if (!string.IsNullOrWhiteSpace(v))
                output = Path.GetFullPath(v);
        }

        static string RequirePath(string[] args, ref int i, string name)
        {
            if (i + 1 >= args.Length)
                throw new ArgumentException($"Missing value for --{name}.");
            i++;
            return Path.GetFullPath(args[i]);
        }

        static void PrintHelp()
        {
            Console.WriteLine("""
                WinDbg Symbols Caching Proxy — MSI builder (WixSharp / WiX 4)

                Options:
                  --server, -s <dir>   Published server output (default: ./publish-x64/server)
                  --agent,  -a <dir>   Published agent output (default: ./publish-x64/agent)
                  --out,    -o <dir>   MSI output directory (default: ./publish-x64/installer)

                Requires Windows, the WiX 4 toolset (dotnet tool install --global wix), and prior dotnet publish of both apps.

                Environment (optional; used by NUKE BuildInstaller):
                  WDSCP_INSTALLER_SERVER, WDSCP_INSTALLER_AGENT, WDSCP_INSTALLER_OUT
                """);
            Environment.Exit(0);
        }

        public void Validate()
        {
            if (!Directory.Exists(ServerPublishDir))
                throw new DirectoryNotFoundException($"Server publish directory not found: {ServerPublishDir}");
            if (!Directory.Exists(AgentPublishDir))
                throw new DirectoryNotFoundException($"Agent publish directory not found: {AgentPublishDir}");

            if (!System.IO.File.Exists(Path.Combine(ServerPublishDir, "WinDbgSymbolsCachingProxy.exe")))
                throw new FileNotFoundException(
                    $"Expected WinDbgSymbolsCachingProxy.exe under server publish directory: {ServerPublishDir}");

            if (!System.IO.File.Exists(Path.Combine(AgentPublishDir, "HarvestingAgent.exe")))
                throw new FileNotFoundException(
                    $"Expected HarvestingAgent.exe under agent publish directory: {AgentPublishDir}");

            Directory.CreateDirectory(OutputDir);
        }
    }
}
