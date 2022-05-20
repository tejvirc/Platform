using System.Collections.Generic;
using System.IO;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Tools.GitVersion;
using static Nuke.Common.ChangeLog.ChangelogTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitVersion.GitVersionTasks;

partial class Build
{
    [Parameter] readonly bool AutoStash = true;

    string ChangelogFile => RootDirectory / "CHANGELOG.md";

    IEnumerable<string> ChangelogSectionNotes => ExtractChangelogSectionNotes(ChangelogFile);

    Target Changelog => _ => _
        .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnHotfixBranch())
        .Executes(() =>
        {
            FinalizeChangelog(ChangelogFile, GitVersion.MajorMinorPatch, GitRepository);
            Git($"add {ChangelogFile}");
            Git($"commit -m \"Finalize {Path.GetFileName(ChangelogFile)} for {GitVersion.MajorMinorPatch}\"");
        });

    Target Release => _ => _
        .DependsOn(Changelog)
        .Requires(() => !GitRepository.IsOnReleaseBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            if (!GitRepository.IsOnReleaseBranch())
                Checkout($"{ReleaseBranchPrefix}/{GitVersion.MajorMinorPatch}", start: MainBranch);
            else
                FinishReleaseOrHotfix();
        });

    Target Hotfix => _ => _
        .DependsOn(Changelog)
        .Requires(() => !GitRepository.IsOnHotfixBranch() || GitHasCleanWorkingCopy())
        .Executes(() =>
        {
            var masterVersion = GitVersion(s => s
                .SetFramework("net5.0")
                .SetUrl(RootDirectory)
                .SetBranch(MainBranch))
                .Result;

            if (!GitRepository.IsOnHotfixBranch())
                Checkout($"{HotfixBranchPrefix}/{masterVersion.Major}.{masterVersion.Minor}.{masterVersion.Patch + 1}", start: MainBranch);
            else
                FinishReleaseOrHotfix();
        });

    void FinishReleaseOrHotfix()
    {
        Git($"checkout {MainBranch}");
        Git($"merge --no-ff --no-edit {GitRepository.Branch}");
        Git($"tag {GitVersion.MajorMinorPatch}");

        Git($"branch -D {GitRepository.Branch}");

        Git($"push origin {MainBranch} {GitVersion.MajorMinorPatch}");
    }

    void Checkout(string branch, string start)
    {
        if (AutoStash)
            Git("git stash");

        Git($"checkout -b {branch} {start}");

        if (AutoStash)
            Git("git stash apply");
    }
}
