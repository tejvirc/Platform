namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Game option config values
    /// </summary>
    public class GameOptionConfigValues
    {
        /// <summary>
        ///     Gets or sets the theme identifier.
        /// </summary>
        /// <value>
        ///     The theme identifier.
        /// </value>
        public string ThemeId { get; set; }

        /// <summary>
        ///     Gets or sets the paytable identifier.
        /// </summary>
        /// <value>
        ///     The paytable identifier.
        /// </value>
        public string PaytableId { get; set; }

        /// <summary>
        ///     Gets or sets the maximum wager credits.
        /// </summary>
        /// <value>
        ///     The maximum wager credits.
        /// </value>
        public int? MaximumWagerCredits { get; set; }

        /// <summary>
        ///     Gets or sets the progressive allowed.
        /// </summary>
        /// <value>
        ///     The progressive allowed.
        /// </value>
        public bool? ProgressiveAllowed { get; set; }

        /// <summary>
        ///     Gets or sets the secondary allowed.
        /// </summary>
        /// <value>
        ///     The secondary allowed.
        /// </value>
        public bool? SecondaryAllowed { get; set; }

        /// <summary>
        ///     Gets or sets the central allowed.
        /// </summary>
        /// <value>
        ///     The central allowed.
        /// </value>
        public bool? CentralAllowed { get; set; }
    }
}