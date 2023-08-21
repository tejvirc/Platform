namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    /// <summary>
    ///     Represents the game start data reported in the 
    ///     game start Sas exception (7E).
    /// </summary>
    public class GameStartData
    {
        /// <summary>
        ///     Gets or sets the credits bet
        /// </summary>
        public long CreditsWagered { get; set; }

        /// <summary>
        ///     Gets or sets the coin in meter (in millicent, after bet)
        /// </summary>
        public long CoinInMeter { get; set; }

        /// <summary>
        ///     Gets or sets the progressive group
        /// </summary>
        public byte ProgressiveGroup { get; set; }

        /// <summary>
        ///     Gets or sets the wager type
        /// </summary>
        public byte WagerType { get; set; }
    }
}
