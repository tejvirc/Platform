namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;
using Contracts.ResponsibleGaming;

public class ResponsibleGamingOptions
{
    public bool TimeLimitEnabled { get; set; }

    public IList<double> TimeLimits { get; } = new List<double>();

    public IList<double> PlayBreaks { get; } = new List<double>();

    public int SessionLimit { get; set; }

    public int Pages { get; set; }

    public ResponsibleGamingExitStrategy ExitStrategy { get; set; }

    public bool PrintHelpline { get; set; }

    public bool FullScreen { get; set; }

    public int Timeout { get; set; }

    public ResponsibleGamingButtonPlacement ButtonPlacement { get; set; }

    public ResponsibleGamingSessionMode SessionMode { get; set; }

    public bool DisplaySessionTimeInClock { get; set; }

    public string? TimeLimitDialogTemplate { get; set; }

    public bool AgeWarningEnabled { get ; set; }

    public string? AgeWarningTemplate { get; set; }
}
