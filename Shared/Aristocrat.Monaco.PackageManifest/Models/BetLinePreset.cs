namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Maps a <see cref="BetOption"/> node to a <see cref="LineOption"/> node.
    /// </summary>
    /// <remarks>
    ///     https://confy.aristocrat.com/display/ConfyOverhaulPOC/%5BBetlines%5D+Changing+Betlines+-+Info+and+Gap+Analysis
    /// </remarks>
    public class BetLinePreset
    {
        /// <summary>
        ///     The unique identifier for BetLinePreset
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The BetOption reference
        /// </summary>
        public BetOption BetOption { get; set; }

        /// <summary>
        ///     The LineOption reference
        /// </summary>
        public LineOption LineOption { get; set; }
    }
}