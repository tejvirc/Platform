namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines a wager category
    /// </summary>
    public class WagerCategory
    {
        /// <summary>
        ///     Gets or sets the Wager category identifier
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///     Gets or sets the theoretical payback percentage associated with the wager category
        /// </summary>
        public decimal TheoPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the minimum wager, in credits, associated with the wager category
        /// </summary>
        public int MinWagerCredits { get; set; }

        /// <summary>
        ///     Gets or sets the maximum wager, in credits, associated with the wager category
        /// </summary>
        public int MaxWagerCredits { get; set; }

        /// <summary>
        ///     Gets or sets the maximum win amount, in millicents, associated with the wager category
        /// </summary>
        public long MaxWinAmount { get; set; }
    }
}
