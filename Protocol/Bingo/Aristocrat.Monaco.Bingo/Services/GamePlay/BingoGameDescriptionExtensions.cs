namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System.Collections.Generic;
    using System.Linq;
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
            var joinBallIndex = bingoGame.JoinBallIndex > 0
                ? bingoGame.JoinBallIndex
                : bingoGame.BallCallNumbers.Count();
            return bingoGame.BallCallNumbers.Take(joinBallIndex);
        }
    }
}