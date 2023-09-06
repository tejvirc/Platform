namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;

public class AttractOptions
{
    public bool HasIdleVideo { get; set; }

    public bool HasIntroVideo { get; set; }

    public bool BottomVideo { get; set; }

    public string? DefaultTopVideoFilename { get; set; }

    public string? DefaultTopperVideoFilename { get; set; }

    public string? NoBonusVideoFilename { get; set; }

    public string? WithBonusVideoFilename { get; set; }

    public string? TopIntroVideoFilename { get; set; }

    public string? BottomIntroVideoFilename { get; set; }

    public string? TopperIntroVideoFilename { get; set; }

    public bool AlternateLanguage { get; set; }

    public int TimerIntervalInSeconds { get; set; }

    public int SecondaryTimerIntervalInSeconds { get; set; }

    public int ConsecutiveVideos { get; set; }

    public bool RotateTopImage { get; set; }

    public bool RotateTopperImage { get; set; }

    public IList<string> TopImageRotation { get; } = new List<string>();

    public IList<string> TopperImageRotation { get; } = new List<string>();
}
