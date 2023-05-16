namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System;

    /// <summary>
    ///     Holds the data for a game outcome
    /// </summary>
    public class GameOutcome : IResponse
    {
        public GameOutcome(
            ResponseCode code,
            GameOutcomeWinDetails winDetails,
            GameOutcomeGameDetails gameDetails,
            GameOutcomeBingoDetails bingoDetails,
            bool isSuccessful,
            bool isFinal,
            int gameId = 0,
            int gameIndex = 0)
        {
            ResponseCode = code;
            WinDetails = winDetails ?? throw new ArgumentNullException(nameof(winDetails));
            GameDetails = gameDetails ?? throw new ArgumentNullException(nameof(gameDetails));
            BingoDetails = bingoDetails ?? throw new ArgumentNullException(nameof(bingoDetails));
            IsSuccessful = isSuccessful;
            IsFinal = isFinal;
            GameId = gameId;
            GameIndex = gameIndex;
        }

        /// <summary>
        ///     The response code for this outcome
        /// </summary>
        public ResponseCode ResponseCode { get; }

        /// <summary>
        ///     The win details for the outcome
        /// </summary>
        public GameOutcomeWinDetails WinDetails { get; }

        /// <summary>
        ///     The game details for the outcome
        /// </summary>
        public GameOutcomeGameDetails GameDetails { get; }

        /// <summary>
        ///     The bingo details for the outcome
        /// </summary>
        public GameOutcomeBingoDetails BingoDetails { get; }

        /// <summary>
        ///     Whether this is the final outcome for the game
        /// </summary>
        public bool IsFinal { get; }

        /// <summary>
        ///     Whether the outcome completed successfully
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        ///     The game id associated with the outcome
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     The game index associated with the outcome
        /// </summary>
        public int GameIndex { get; }
    }
}