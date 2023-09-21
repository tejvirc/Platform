namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
using System.Collections.Immutable;
using Contracts.InfoBar;
using Gaming.Contracts.InfoBar;

public record InfoBarState
{
    public bool MainInfoBarOpenRequested { get; init; }

    public bool VbdInfoBarOpenRequested { get; init; }

    public bool IsOpen { get; init; }

    public string? LeftRegionText { get; init; }

    public string? CenterRegionText { get; init; }

    public string? RightRegionText { get; init; }

    public double LeftRegionDuration { get; init; }

    public double CenterRegionDuration { get; init; }

    public double RightRegionDuration { get; init; }

    public InfoBarColor BackgroundColor { get; init; }

    public InfoBarColor LeftRegionTextColor { get; init; }

    public InfoBarColor CenterRegionTextColor { get; init; }

    public InfoBarColor RightRegionTextColor { get; init; }

    public ImmutableList<InfoBarMessageData> MessageDataSet { get; init; }
}