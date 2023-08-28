namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;

public class LocalizationOptions
{
    public bool MultiLanguage { get; set; }

    public IList<string> LocaleCodes { get; } = new List<string>();
}
