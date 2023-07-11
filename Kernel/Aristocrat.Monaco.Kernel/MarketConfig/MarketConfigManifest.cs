namespace Aristocrat.Monaco.Kernel.MarketConfig;

using System.Collections.Generic;

/// <summary>
///     Model object that maps to the manifest.json file exported by the configuration tool.
/// </summary>
public class MarketConfigManifest
{
    /// <summary>
    ///     List of jurisdictions that were exported from the configuration tool.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<MarketConfigManifestJurisdiction> Jurisdictions { get; set; } = new();
}
