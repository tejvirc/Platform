namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Determine which placements in the reel stops create a "win".
    /// </summary>
    /// <remarks>
    ///     https://confy.aristocrat.com/display/ConfyOverhaulPOC/%5BBetlines%5D+Changing+Betlines+-+Info+and+Gap+Analysis
    /// </remarks>
    public class Line
    {
        /// <summary>
        ///     The cost in credits of the playline without applicable multiplier.
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        ///     The total cost in credits of the playline.
        /// </summary>
        public int TotalCost => Cost * Multiplier;

        /// <summary>
        ///     The multiplier of the playline. 
        /// </summary>
        public int Multiplier { get; set; } = 1;

        /// <summary>
        ///     Button determines the mapping to the button-deck buttons.
        /// </summary>
        public string Button { get; set; }

        /// <summary>
        ///     ButtonName is text that can be displayed on the button for VBD and LCD button decks.
        /// </summary>
        public string ButtonName { get; set; }

    }
}