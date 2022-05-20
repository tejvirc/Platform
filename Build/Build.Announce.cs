using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.Utilities;
using Nuke.Common.Tools.Slack;
using static Nuke.Common.Tools.Slack.SlackTasks;

partial class Build
{
    [Parameter("Slack Webhook")] readonly string SlackWebhook;

    Target Announce => _ => _
        .AssuredAfterFailure()
        .OnlyWhenStatic(() => GitRepository.IsOnMainBranch())
        .Executes(() =>
        {
            SendSlackMessage(_ => _
                    .SetText(new StringBuilder()
                        .AppendLine($"<!here> :mega::shipit: *Monaco Platform {GitVersion.SemVer} IS OUT!!!*")
                        .AppendLine()
                        .AppendLine(ChangelogSectionNotes.Select(x => x.Replace("- ", "• ")).JoinNewLine()).ToString()),
                SlackWebhook);
        });
}
