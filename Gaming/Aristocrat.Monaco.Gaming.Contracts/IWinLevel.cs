namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     A win level is associated with one or more specific winning combinations within a paytable.A win level may be tied
    ///     to a progressive jackpot.Each win level is assigned a win level index.The combination of the themeId, paytableId,
    ///     winLevelIndex, denomId, and numberOfCredits may be mapped to a specific level within a progressive.
    /// </summary>
    public interface IWinLevel
    {
        /// <summary>
        ///     Gets the zero-based unique paytable index for the win level.
        /// </summary>
        /// <value>The the zero-based unique paytable index for the win level.</value>
        int WinLevelIndex { get; }

        /// <summary>
        ///     Gets the description of the paytable win level.
        /// </summary>
        /// <value>The description of the paytable win level.</value>
        string WinLevelCombo { get; }

        /// <summary>
        ///     Gets a value indicating whether progressive is allowed.
        /// </summary>
        /// <value>true if progressive allowed, false if not.</value>
        bool ProgressiveAllowed { get; }
    }
}