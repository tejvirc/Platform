namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Bet details specify various aspects of a game round wager.
    /// </summary>
    public interface IBetDetails
    {
        /// <summary>
        ///     Gets the bet line preset ID.
        /// </summary>
        int BetLinePresetId { get; }

        /// <summary>
        ///     Gets the bet per line
        /// </summary>
        int BetPerLine { get; }

        /// <summary>
        ///     Gets the number of lines.
        /// </summary>
        int NumberLines { get; }

        /// <summary>
        ///     Gets the ante.
        /// </summary>
        int Ante { get; }

        /// <summary>
        ///     Gets the stake
        /// </summary>
        long Stake { get; }

        /// <summary>
        ///     Gets the wager
        /// </summary>
        long Wager { get; }

        /// <summary>
        ///     Gets the bet multiplier
        /// </summary>
        int BetMultiplier { get; }

        /// <summary>
        ///     Gets the unique game ID
        /// </summary>
        int GameId { get; }
    }
}