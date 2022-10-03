namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Common.GameOverlay;
    using Common.Storage;

    /// <summary>
    ///     Extension methods for BingoGameDescription
    /// </summary>
    public static class BingoGameDescriptionExtensions
    {
        /// <summary>
        ///     Gets the balls that occurred when the game was initially joined
        /// </summary>
        /// <param name="bingoGame">The bingo game description to get the joined balls</param>
        /// <returns>The joined bingo numbers for the game</returns>
        public static IEnumerable<BingoNumber> GetJoiningBalls(this BingoGameDescription bingoGame)
        {
            var joinBallIndex = bingoGame.GetGameStartBallIndex();
            return bingoGame.BallCallNumbers.Take(joinBallIndex);
        }

        /// <summary>
        ///     Gets the game starting ball index.  For a new game the join index will be zero this corrects
        ///     for what the balls where at the start of the game.
        /// </summary>
        /// <param name="bingoGame">The bingo game description to get the joined balls</param>
        /// <returns>The ball index for what existed at the time of evaluation of the paytable</returns>
        public static int GetGameStartBallIndex(this BingoGameDescription bingoGame)
        {
            return bingoGame.JoinBallIndex <= 0 ? BingoConstants.InitialBallDraw : bingoGame.JoinBallIndex;
        }
    }
}