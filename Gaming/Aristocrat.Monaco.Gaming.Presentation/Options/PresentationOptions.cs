namespace Aristocrat.Monaco.Gaming.Presentation.Options;

using System.Collections.Generic;

public class PresentationOptions
{
    public string? AssetsPath { get; set; }

    public IList<string> SkinFiles { get; } = new List<string>();
}
