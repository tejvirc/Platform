namespace Aristocrat.G2S.Emdi.Extensions
{
    using Monaco.Gaming.Contracts;
    using Protocol.v21ext1b1;
    using System;

    /// <summary>
    /// Extension methods for game play-related commands
    /// </summary>
    public static class GamePlayExtensions
    {
        /// <summary>
        ///     Converts an <see cref="IGameHistoryLog" /> instance to a <see cref="recallLog" />
        /// </summary>
        /// <param name="this">The <see cref="IGameHistoryLog" /> instance to convert.</param>
        /// <returns>A <see cref="recallLog" /> instance.</returns>
        public static recallLog ToRecallLog(this IGameHistoryLog @this)
        {
            return new recallLog
            {
                // todo themeIdField = "",
                // todo paytableId = "",
                denomId = @this.DenomId,
                gameDateTime = @this.LastUpdate,
                playResult = ToGamePlayResult(@this.Result),
                initialWager = @this.InitialWager * GamingConstants.Millicents,
                finalWager = @this.FinalWager * GamingConstants.Millicents,
                initialWin = @this.InitialWin * GamingConstants.Millicents,
                secondaryPlayed = @this.SecondaryPlayed,
                secondaryWager = @this.SecondaryWager * GamingConstants.Millicents,
                secondaryWin = @this.SecondaryWin * GamingConstants.Millicents,
                finalWin = @this.FinalWin * GamingConstants.Millicents
            };
        }

        private static t_playResults ToGamePlayResult(GameResult result)
        {
            switch (result)
            {
                case GameResult.None:
                    return t_playResults.G2S_noResult;
                case GameResult.Failed:
                    return t_playResults.G2S_gameFailed;
                case GameResult.Lost:
                    return t_playResults.G2S_gameLost;
                case GameResult.Tied:
                    return t_playResults.G2S_gameTied;
                case GameResult.Won:
                    return t_playResults.G2S_gameWon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }
    }
}