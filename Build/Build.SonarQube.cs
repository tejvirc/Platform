using Nuke.Common;
using Nuke.Common.Tools.SonarScanner;
using static Nuke.Common.Tools.SonarScanner.SonarScannerTasks;

partial class Build
{
    [Parameter("SonarQube server host url")]
    string SonarServer;

    [Parameter("SonarQube login/token")]
    string SonarLogin;

    [Parameter("SonarQube unique project key")]
    string SonarProjectKey;

    Target Analyze => _ => _
        .Requires(() => SonarServer, () => SonarLogin, () => SonarProjectKey)
        .Before(Coverage)
        .Triggers(Coverage, AnalyzeEnd)
        .Executes(() =>
        {
            SonarScannerBegin(_ => _
                .SetFramework("net5.0")
                .SetProjectKey(SonarProjectKey)
                .SetVersion(GitVersion.SemVer)
                .SetLogin(SonarLogin)
                .SetServer(SonarServer)
                .SetVSTestReports("**/*.trx")
                .SetOpenCoverPaths("**/*.coverage.xml"));
        });

    Target AnalyzeEnd => _ => _
        .Requires(() => SonarLogin)
        .Unlisted()
        .After(Coverage)
        .Consumes(Test, TestResultsDirectory / "*.trx", TestResultsDirectory / "*.xml")
        .Executes(() =>
        {
            SonarScannerEnd(s => s
                .SetFramework("net5.0")
                .SetLogin(SonarLogin));
        });
}
