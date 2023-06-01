namespace Aristocrat.Monaco.Kernel.MarketConfig;

using System.Collections.Generic;

/// <summary>
///     Model object that maps to the manifest.json file exported by the configuration tool.
///     <seealso cref="MarketConfigManifest"/>
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class MarketConfigManifestJurisdiction
{
    /// <summary>
    ///     The jurisdiction machine ID from the configuration tool.
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    ///     The legacy jurisdiction ID from the addins jurisdiction files.
    /// </summary>
    public string JurisdictionInstallationId { get; set; } = string.Empty;

    /// <summary>
    ///     The display label for the jurisdiction.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     A dictionary of the configuration tool segments and the relative path of the json files exported from the
    ///     configuration tool for this jurisdiction. The key is the segment ID and the value is the relative path to
    ///     the json file.
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> Segments { get; set; } = new();
}
