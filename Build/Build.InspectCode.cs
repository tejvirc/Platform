using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.ReSharper;
using static Nuke.Common.Tools.ReSharper.ReSharperTasks;

partial class Build
{
    AbsolutePath InspectResultsDirectory => OutputDirectory / "InspectResults";

    AbsolutePath InspectResultsFile => InspectResultsDirectory / "InspectResults.xml";

    Target Inspect => _ => _
        // .DependsOn(Compile)
        .TriggeredBy(Analyze)
        .After(Analyze)
        .Executes(
            () =>
            {
                ReSharperInspectCode(_ => _
                    .SetTargetPath(Solution)
                    .SetOutput(InspectResultsFile)
                    .SetSeverity(ReSharperSeverity.WARNING)
                    .SetToolset("16.0")
                    .SetNoSwea(false)
                    .SetProperty("Platform", "x64")
                    .SetProperty("Configuration", Configuration)
                    .SetExclude("Test", "Build", "Tasks"));
            });
}
