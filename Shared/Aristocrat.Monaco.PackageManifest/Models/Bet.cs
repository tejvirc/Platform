namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     The buttons within the <see cref="BetOption"/> set.
    /// </summary>
    /// <remarks>
    ///     https://confy.aristocrat.com/display/ConfyOverhaulPOC/%5BBetlines%5D+Changing+Betlines+-+Info+and+Gap+Analysis
    /// </remarks>
    public class Bet
    {
        /// <summary>
        ///     The bet multiplier.
        /// </summary>
        public int Multiplier { get; set; }

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
