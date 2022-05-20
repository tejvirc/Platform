namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     A wager category is a subset of a paytable with a specific payback percentage. A paytable may be made up of one or
    ///     more wager categories.Wager categories may be based on the amount wagered and/or the type of wager.
    /// </summary>
    public interface IWagerCategory
    {
        /// <summary>
        ///     Gets the identifier of the wager category.
        /// </summary>
        /// <value>
        ///     The identifier of the wager category.
        /// </value>
        string Id { get; }

        /// <summary>
        ///     Gets the theo payback pct.
        /// </summary>
        /// <value>
        ///     The theo payback pct.
        /// </value>
        decimal TheoPaybackPercent { get; }

        /// <summary>
        ///     Gets the minimum wager credits.
        /// </summary>
        /// <value>
        ///     The minimum wager credits.
        /// </value>
        int? MinWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum wager credits.
        /// </summary>
        /// <value>
        ///     The maximum wager credits.
        /// </value>
        int? MaxWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum win amount.
        /// </summary>
        /// <value>
        ///     The maximum win amount.
        /// </value>
        long MaxWinAmount { get; }
    }
}