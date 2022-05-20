using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.Tools.NuGet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.Tools.DocFX.DocFXTasks;
using static Nuke.Common.Tools.NuGet.NuGetTasks;

partial class Build
{
    AbsolutePath DocsDirectory => RootDirectory / "Documentation";

    AbsolutePath DocsPluginsDirectory => DocsDirectory / "plugins";

    string DocFxFile => DocsDirectory / "docfx.json";

    IEnumerable<string> DocFxPlugins => new[] {
        "DocFx.Plugins.PlantUml",
        "DocFx.Plugins.Monaco"
    };

    Target DownloadDocPlugins => _ => _
        .Unlisted()
        .WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            var packages = DocFxPlugins.Select(x => x);
            packages.ForEach(x =>
                NuGetInstall(_ => _
                    .SetOutputDirectory(DocsPluginsDirectory)
                    .SetExcludeVersion(true)
                    .SetDependencyVersion(DependencyVersion.Ignore)
                    .SetVerbosity(NuGetVerbosity.Detailed)));
        });

    Target BuildDocMetadata => _ => _
        .DependsOn(DownloadDocPlugins)
        .Unlisted()
        .WhenSkipped(DependencyBehavior.Skip)
        .Executes(() =>
        {
            DocFXMetadata(s => s
                .SetProjects(DocFxFile)
                .SetLogLevel(DocFXLogLevel.Verbose));
        });

    Target BuildDocs => _ => _
        .DependsOn(BuildDocMetadata)
        .Executes(() =>
        {
            DocFXBuild(s => s
                .SetConfigFile(DocFxFile)
                .SetLogLevel(DocFXLogLevel.Verbose));
        });
}
