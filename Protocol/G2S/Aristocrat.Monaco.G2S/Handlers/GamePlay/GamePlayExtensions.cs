namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;

    /// <summary>
    ///     Extension methods for game play-related commands
    /// </summary>
    public static class GamePlayExtensions
    {
        /// <summary>
        ///     Converts an <see cref="IGameHistoryLog" /> instance to a <see cref="recallLog" />
        /// </summary>
        /// <param name="this">The <see cref="IGameHistoryLog" /> instance to convert.</param>
        /// <returns>A <see cref="printLog" /> instance.</returns>
        public static recallLog ToRecallLog(this IGameHistoryLog @this)
        {
            return new recallLog
            {
                logSequence = @this.LogSequence,
                deviceId = @this.GameId,
                transactionId = @this.TransactionId,
                gameDateTime = @this.LastUpdate,
                playState = ToGamePlayState(@this.PlayState),
                playResult = ToGamePlayResult(@this.Result),
                denomId = @this.DenomId,
                initialWager = @this.InitialWager * GamingConstants.Millicents,
                finalWager = @this.FinalWager * GamingConstants.Millicents,
                initialWin = @this.InitialWin * GamingConstants.Millicents,
                secondaryPlayed = @this.SecondaryPlayed,
                secondaryWin = @this.SecondaryWin * GamingConstants.Millicents,
                secondaryWager = @this.SecondaryWager * GamingConstants.Millicents,
                finalWin = @this.TotalWon * GamingConstants.Millicents,
                winLevelList = @this.Result == GameResult.Won ? EmptyWinLevelList() : null
            };
        }

        /// <summary>
        ///     Set the winLevelItem to a valid, but empty list
        /// </summary>
        /// <param name="this">A <see cref="gamePlayProfile" /> instance.</param>
        public static void EmptyWinLevelList(this gamePlayProfile @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            @this.winLevelList = new winLevelList
            {
                winLevelItem = new List<winLevelItem> { new winLevelItem { winLevelIndex = 0 } }.ToArray()
            };
        }

        private static winLevelList EmptyWinLevelList()
        {
            return new winLevelList
            {
                winLevelItem = new List<winLevelItem> { new winLevelItem { winLevelIndex = 0 } }.ToArray()
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

        private static t_playStates ToGamePlayState(PlayState state)
        {
            switch (state)
            {
                case PlayState.Idle:
                case PlayState.Initiated:
                    return t_playStates.G2S_gameIdle;
                case PlayState.PrimaryGameEscrow:
                    return t_playStates.G2S_primaryGameEscrow;
                case PlayState.PrimaryGameStarted:
                    return t_playStates.G2S_primaryGameStarted;
                case PlayState.PrimaryGameEnded:
                    return t_playStates.G2S_primaryGameEnded;
                case PlayState.ProgressivePending:
                    return t_playStates.G2S_progressivePending;
                case PlayState.SecondaryGameChoice:
                    return t_playStates.G2S_secondaryGameChoice;
                case PlayState.SecondaryGameEscrow:
                    return t_playStates.G2S_secondaryGameEscrow;
                case PlayState.SecondaryGameStarted:
                    return t_playStates.G2S_secondaryGameStarted;
                case PlayState.SecondaryGameEnded:
                    return t_playStates.G2S_secondaryGameEnded;
                case PlayState.PayGameResults:
                    return t_playStates.G2S_payGameResults;
                case PlayState.GameEnded:
                case PlayState.FatalError:
                    return t_playStates.G2S_gameEnded;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}