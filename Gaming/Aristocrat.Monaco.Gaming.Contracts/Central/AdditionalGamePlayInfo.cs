namespace Aristocrat.Monaco.Gaming.Contracts.Central
{
    /// <summary>
    ///     Holds the data for additional game plays, such as side bet and wonder 4
    /// </summary>
    public class AdditionalGamePlayInfo : IAdditionalGamePlayInfo
    {
        /// <summary>
        ///     Initializes a new instance of the AdditionalGamePlayInfo class
        /// </summary>
        /// <param name="gameIndex">The game index</param>
        /// <param name="gameId">The game id</param>
        /// <param name="denomination">The denomination of the game</param>
        /// <param name="wagerAmount">The wager amount for this game play</param>
        /// <param name="templateId"></param>
        public AdditionalGamePlayInfo(int gameIndex, int gameId, long denomination, long wagerAmount, int templateId)
        {
            GameIndex = gameIndex;
            GameId = gameId;
            Denomination = denomination;
            WagerAmount = wagerAmount;
            TemplateId = templateId;
        }

        /// <inheritdoc/>
        public int GameIndex { get; set; }

        /// <inheritdoc/>
        public int GameId { get; set; }

        /// <inheritdoc/>
        public long Denomination { get; set; }

        /// <inheritdoc/>
        public long WagerAmount { get; set; }

        /// <inheritdoc/>
        public int TemplateId { get; }
    }
}