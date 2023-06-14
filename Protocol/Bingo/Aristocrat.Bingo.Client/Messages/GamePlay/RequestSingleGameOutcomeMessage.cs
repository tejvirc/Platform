namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    /// <summary>
    ///     Holds information for one game play
    /// </summary>
    public class RequestSingleGameOutcomeMessage
    {
        /// <summary>
        ///     Creates a new instance of the RequestSingleGameOutcomeMessage class
        /// </summary>
        /// <param name="gameIndex">A unique id for a game</param>
        /// <param name="betAmount">The bet amount for the game play</param>
        /// <param name="activeDenomination">The denomination for the bet</param>
        /// <param name="betLinePresetId">The bet line preset id</param>
        /// <param name="lineBet">The line bet</param>
        /// <param name="lines">The number of lines</param>
        /// <param name="ante">The ante bet</param>
        /// <param name="activeGameTitle">The active game title</param>
        /// <param name="uniqueGameId">The unique game id/param>
        public RequestSingleGameOutcomeMessage(
            int gameIndex,
            long betAmount,
            long activeDenomination,
            int betLinePresetId,
            int lineBet,
            int lines,
            long ante,
            int activeGameTitle,
            int uniqueGameId)
        {
            GameIndex = gameIndex;
            BetAmount = betAmount;
            ActiveDenomination = activeDenomination;
            BetLinePresetId = betLinePresetId;
            LineBet = lineBet;
            Lines = lines;
            Ante = ante;
            ActiveGameTitle = activeGameTitle;
            UniqueGameId = uniqueGameId;
        }

        /// <summary>
        ///     Gets the unique number for this game
        /// </summary>
        public int GameIndex { get; }

        /// <summary>
        ///     Gets the bet amount for the game
        /// </summary>
        public long BetAmount { get; }

        /// <summary>
        ///     Gets the active denomination for the bet
        /// </summary>
        public long ActiveDenomination { get; }

        /// <summary>
        ///     Gets the bet line preset id for games that support lines
        /// </summary>
        public int BetLinePresetId { get; }

        /// <summary>
        ///     Gets the line bet for games that support lines
        /// </summary>
        public int LineBet { get; }

        /// <summary>
        ///     Gets the lines for games that support lines
        /// </summary>
        public int Lines { get; }

        /// <summary>
        ///     Gets the ante for games that support ante bets
        /// </summary>
        public long Ante { get; }

        /// <summary>
        ///     Gets the active game title
        /// </summary>
        public int ActiveGameTitle { get; }

        /// <summary>
        ///     Gets the unique game id
        /// </summary>
        public int UniqueGameId { get; }
    }
}