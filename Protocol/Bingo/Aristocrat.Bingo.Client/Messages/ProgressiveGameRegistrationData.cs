namespace Aristocrat.Bingo.Client.Messages
{
    /// <summary>
    ///     Definition of ProgressiveGameRegistrationData
    /// </summary>
    public class ProgressiveGameRegistrationData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ProgressiveGameRegistrationData"/> class.
        /// </summary>
        /// <param name="gameTitleId">The game title id</param>
        /// <param name="denomination">The game denomination in cents</param>
        /// <param name="maxBet">The game max bet in cents</param>
        public ProgressiveGameRegistrationData(int gameTitleId, int denomination, int maxBet)
        {
            GameTitleId = gameTitleId;
            Denomination = denomination;
            MaxBet = maxBet;
        }

        /// <summary>
        ///     The game title id
        /// </summary>
        public int GameTitleId { get; }

        /// <summary>
        ///     The game denomination in cents
        /// </summary>
        public int Denomination { get; }

        /// <summary>
        ///     The game max bet in cents
        /// </summary>
        public int MaxBet { get; }
    }
}