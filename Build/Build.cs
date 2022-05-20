using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.Coverlet.CoverletTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
partial class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Produce deterministic builds")]
    bool Deterministic;

    [CI] readonly GitHubActions GitHubActions;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(NoFetch = true, UpdateBuildNumber = true, Framework = "net5.0")] readonly GitVersion GitVersion;

    AbsolutePath TestsDirectory => RootDirectory / "Test";

    AbsolutePath OutputDirectory => RootDirectory / "bin";

    const string MainBranch = "main";
    const string ReleaseBranchPrefix = "release";
    const string HotfixBranchPrefix = "hotfix";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            TestsDirectory.GlobDirectories("**/bin", "**/obj")
                .ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            NuGetRestore(_ => _
                .SetTargetPath(Solution));

            DotNetToolRestore();
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                // .SetOutDir(OutputDirectory)
                .SetProcessEnvironmentVariable("EnlistmentRoot", RootDirectory)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer) // Version setting methods do not appear to be working correctly
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .SetMaxCpuCount(1) // Monaco Platform has issues with parallel build
                .SetRestore(false)
                .When(Deterministic, _ => _
                    .SetProperty("Deterministic", true)
                    .SetProperty("PathMap", "$(EnlistmentRoot)=C:\\")));

            GitHubActions?.WriteCommand("set-output", GitVersion.AssemblySemVer, dictionaryConfigurator: c =>
            {
                c.Add("name", "assemblySemVer");
                return c;
            });

            GitHubActions?.WriteCommand("set-output", GitVersion.SemVer, dictionaryConfigurator: c =>
            {
                c.Add("name", "semVer");
                return c;
            });
        });

    AbsolutePath TestResultsDirectory => OutputDirectory / "TestResults";

    IEnumerable<AbsolutePath> TestAssemblies => TestsDirectory.GlobFiles(@"**\bin\Debug\*.Tests.dll");

    Target Test => _ => _
        .DependsOn(Compile)
        .Produces(TestResultsDirectory / "*.trx")
        .Produces(TestResultsDirectory / "*.xml")
        .Executes(() =>
        {
            Coverlet(_ => _
                .SetExclude("[*.Tests]*")
                .SetExcludeByFile("*.generated.cs")
                .SetProcessWorkingDirectory(OutputDirectory)
                .SetFormat(CoverletOutputFormat.opencover)
                .CombineWith(
                    TestAssemblies.Select(
                        x => new { Path = x.ToString(), Name = Path.GetFileNameWithoutExtension(x) }), (_, v) => _
                        .SetAssembly(v.Path)
                        .SetOutput(TestResultsDirectory / $"{v.Name}.coverage.xml")
                        .SetTargetSettings(new VSTestSettings()
                            .SetTestAssemblies(v.Path)
                            .SetPlatform(VsTestPlatform.x64)
                            .SetLogger($"trx;LogFileName={v.Name}.trx")
                            .SetProcessArgumentConfigurator(args => args
                                .Add("/ResultsDirectory:{0}", TestResultsDirectory)))),
                completeOnFailure: true);
        });

    AbsolutePath CoverageReportDirectory => OutputDirectory / "CoverageReport";

    Target Coverage => _ => _
        .DependsOn(Test)
        .Produces(CoverageReportDirectory / "*.xml")
        .Consumes(Test, TestResultsDirectory / "*.xml")
        .Executes(() =>
        {
            ReportGenerator(c => c
                .SetFramework("net5.0")
                .SetReports(TestResultsDirectory.GlobFiles(@"**\*.xml").Select(x => x.ToString()))
                .SetTargetDirectory(CoverageReportDirectory)
                .SetReportTypes(ReportTypes.Clover, ReportTypes.MarkdownSummary));
        });
}
