namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;

public class LobbyOptions
{
    public bool MidKnightTheme { get; set; }

    public string? TopperVideoFilename { get; set; }

    public bool DisplayPaidMeter { get; set; }

    public string? DefaultLoadingScreenFilename { get; set; }

    public string? DefaultTopIntroVideoFilename { get; set; }

    public bool DisplaySoftErrors { get; set; }

    public bool DisplayInformationMessages { get; set; }

    public bool HideIdleTextOnCashIn { get; set; }
}
