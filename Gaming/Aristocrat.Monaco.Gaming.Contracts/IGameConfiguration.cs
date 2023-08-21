namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Defines the game configuration options
    /// </summary>
    public interface IGameConfiguration
    {
        /// <summary>
        ///     Gets the minimum wager
        /// </summary>
        int MinimumWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum wager
        /// </summary>
        int MaximumWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum wager for low-odds bets, for example betting in roulette on just red/black or odd/even
        /// </summary>
        int MaximumWagerOutsideCredits { get; }

        /// <summary>
        ///     Gets the Bet Option
        /// </summary>
        string BetOption { get; }

        /// <summary>
        ///     Gets the Line Option
        /// </summary>
        string LineOption { get; }

        /// <summary>
        ///     Gets the Bonus bet
        /// </summary>
        int BonusBet { get; }

        /// <summary>
        ///     Gets whether or not the gamble feature defaulted to enabled
        /// </summary>
        bool SecondaryEnabled { get; }

        /// <summary>
        ///     Gets whether or not the Let It Ride feature defaulted to enabled
        /// </summary>
        bool LetItRideEnabled { get; }
    }
}