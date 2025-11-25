using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Docker image name to build and push")]
    readonly string DockerImage = "nefarius.azurecr.io/wdscp:latest";

    const string PublishProfile = "Properties/PublishProfiles/release-win-x64.pubxml";
    const string PublishRuntime = "win-x64";

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

    Target PublishLocal => _ => _
        .Description("Publish server and agent locally using the Release configuration and win-x64 runtime")
        .Executes(() =>
        {
            PublishProject(Solution.GetProject("WinDbgSymbolsCachingProxy"));
            PublishProject(Solution.GetProject("HarvestingAgent"));
        });

    Target PublishRemote => _ => _
        .Description("Build and push the Docker image")
        .Executes(() =>
        {
            DockerTasks.DockerBuild(s => s
                .SetPath(RootDirectory)
                .SetTag(DockerImage));

            DockerTasks.DockerPush(s => s
                .SetName(DockerImage));
        });

    void PublishProject(Project project)
    {
        DotNetTasks.DotNetPublish(s => s
            .SetProject(project)
            .SetConfiguration(Configuration.Release)
            .SetRuntime(PublishRuntime)
            .SetPublishProfile(PublishProfile));
    }

}
