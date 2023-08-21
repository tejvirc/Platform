namespace Aristocrat.Monaco.Kernel.MarketConfig;

/// <summary>
///     Brief summary of a market config jurisdiction.
/// </summary>
public class MarketConfigJurisdictionInfo
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
    ///     Comma separated list of smart card DRM identifiers for this jurisdiction.
    /// </summary>
    public string DrmIdentifiers { get; set; } = string.Empty;
}
