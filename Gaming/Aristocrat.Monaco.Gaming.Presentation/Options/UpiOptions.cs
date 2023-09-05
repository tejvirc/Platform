namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;
using ClockMode = Contracts.Clock.ClockMode;

public class UpiOptions
{
    public string? Template { get; set; }

    public IList<string> LanguageButtonResourceKeys { get; set; } = new List<string>();

    public ClockMode ClockMode { get; set; }
}
