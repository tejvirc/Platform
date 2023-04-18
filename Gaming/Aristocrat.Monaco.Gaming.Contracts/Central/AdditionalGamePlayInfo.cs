namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    using System.Collections.Generic;

    /// <summary>
    ///     Holds the data for additional game plays, such as side bet and wonder 4
    /// </summary>
    public class AdditionalGamePlayInfo : IAdditionalGamePlayInfo
    {
        /// <summary>
        ///     Initializes a new instance of the AdditionalGamePlayInfo class
        /// </summary>
        /// <param name="gameIndex">The game index</param>
        /// <param name="denomination">The denomination of the game</param>
        /// <param name="wagerAmount">The wager amount for this game play</param>
        public AdditionalGamePlayInfo(int gameIndex, long denomination, long wagerAmount)
        {
            GameIndex = gameIndex;
            Denomination = denomination;
            WagerAmount = wagerAmount;
        }

        /// <inheritdoc/>
        public int GameIndex { get; set; }

        /// <inheritdoc/>
        public long Denomination { get; set; }

        /// <inheritdoc/>
        public long WagerAmount { get; set; }
    }
}