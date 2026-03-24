using System;
using System.Linq;

using JetBrains.Annotations;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///     - JetBrains ReSharper        https://nuke.build/resharper
    ///     - JetBrains Rider            https://nuke.build/rider
    ///     - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///     - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Docker image name to build and push")]
    readonly string DockerImage = "nefarius.azurecr.io/wdscp:latest";

    const string PublishRuntime = "win-x64";

    static AbsolutePath PublishRoot => RootDirectory / "publish-x64";
    static AbsolutePath PublishServerDir => PublishRoot / "server";
    static AbsolutePath PublishAgentDir => PublishRoot / "agent";
    static AbsolutePath InstallerOutputDir => PublishRoot / "installer";
    static AbsolutePath InstallerProjectFile => RootDirectory / "installer" / "WinDbgSymbolsCachingProxy.Installer.csproj";

    [Solution] readonly Solution Solution;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
        });

    Target Restore => _ => _
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
        });

    [UsedImplicitly]
    public Target PublishLocal => _ => _
        .Description("Publish server and agent locally using the Release configuration and win-x64 runtime")
        .Executes(() =>
        {
            PublishProject(
                Solution.GetAllProjects("WinDbgSymbolsCachingProxy").Single(),
                PublishServerDir);

            PublishProject(
                Solution.GetAllProjects("HarvestingAgent").Single(),
                PublishAgentDir);
        });

    [UsedImplicitly]
    public Target BuildInstaller => _ => _
        .Description(
            "Publish server and agent (win-x64 Release), then build the x64 MSI via WixSharp. Requires Windows and WiX 4 (dotnet tool install --global wix).")
        .DependsOn(PublishLocal)
        .Executes(() =>
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException(
                    "BuildInstaller must run on Windows with the WiX 4 CLI available (for example: dotnet tool install --global wix).");
            }

            // dotnet run forwards SetApplicationArguments as a single argv; use env vars so the installer sees three paths.
            const string envServer = "WDSCP_INSTALLER_SERVER";
            const string envAgent = "WDSCP_INSTALLER_AGENT";
            const string envOut = "WDSCP_INSTALLER_OUT";
            try
            {
                Environment.SetEnvironmentVariable(envServer, PublishServerDir);
                Environment.SetEnvironmentVariable(envAgent, PublishAgentDir);
                Environment.SetEnvironmentVariable(envOut, InstallerOutputDir);

                DotNetTasks.DotNetRun(s => s
                    .SetProjectFile(InstallerProjectFile)
                    .SetConfiguration(Configuration.Release));
            }
            finally
            {
                Environment.SetEnvironmentVariable(envServer, null);
                Environment.SetEnvironmentVariable(envAgent, null);
                Environment.SetEnvironmentVariable(envOut, null);
            }
        });

    [UsedImplicitly]
    public Target PublishRemote => _ => _
        .Description("Build and push the Docker image")
        .Executes(() =>
        {
            DockerTasks.DockerBuild(s => s
                .SetPath(RootDirectory)
                .SetTag(DockerImage));

            DockerTasks.DockerPush(s => s
                .SetName(DockerImage));
        });

    /// <summary>
    ///     Matches the former <c>release-win-x64.pubxml</c> settings (framework-dependent, R2R, trimmed publish output).
    /// </summary>
    void PublishProject(Project project, AbsolutePath publishDirectory)
    {
        DotNetTasks.DotNetPublish(s => s
            .SetProject(project)
            .SetConfiguration(Configuration.Release)
            .SetRuntime(PublishRuntime)
            .SetOutput(publishDirectory)
            .AddProperty("PublishReadyToRun", "true")
            .AddProperty("PublishSingleFile", "false")
            .AddProperty("SelfContained", "false")
            .AddProperty("DebugType", "none")
            .AddProperty("GenerateDocumentationFile", "false")
            .AddProperty("AllowedReferenceRelatedFileExtensions", "none"));
    }
}
