namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;

public class ChooserOptions
{
    public bool LargeGameIcons { get; set; }

    public double DaysAsNew { get; set; }

    public IList<string> DefaultGameDisplayOrderByThemeId { get; set; } = new List<string>();

    public IList<string> DefaultEnabledGameOrderLightningLink { get; set; } = new List<string>();

    public IList<string> DefaultDisabledGameOrderLightningLink { get; set; } = new List<string>();

    public int MaxDisplayedGames { get; set; }

    public bool PreserveGameLayoutSideMargins { get; set; }

    public bool MinimumWagerCreditsAsFilter { get; set; }
}
