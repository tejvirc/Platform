namespace Aristocrat.Monaco.G2S.Handlers.Progressive
{
    using System.Linq;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     A progressive extensions.
    /// </summary>
    public static class ProgressiveExtensions
    {
        /// <summary>
        ///     A JackpotTransaction extension method that converts this instance to a progressive log.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="gameProvider">The game provider.</param>
        /// <returns>The given data converted to a progressiveLog.</returns>
        public static progressiveLog ToProgressiveLog(this JackpotTransaction @this, IGameProvider gameProvider)
        {
            var gameProfile = gameProvider.GetGame(@this.GameId);
            return new progressiveLog
            {
                logSequence = @this.LogSequence,
                progId = @this.ProgressiveId,
                deviceId = @this.DeviceId,
                transactionId = @this.TransactionId,
                levelId = @this.LevelId,
                progState = (t_progStates)@this.State,
                gamePlayId = @this.GameId,
                themeId = gameProfile?.ThemeId,
                paytableId = gameProfile?.PaytableId,
                denomId = @this.DenomId,
                winLevelIndex = @this.WinLevelIndex,
                winLevelCombo = gameProfile?.WinLevels
                    .SingleOrDefault(a => a.WinLevelIndex == @this.WinLevelIndex)?.WinLevelCombo,
                progValueAmt = @this.ValueAmount,
                progValueText = @this.ValueText,
                progValueSeq = @this.ValueSequence,
                hitDateTime = @this.TransactionDateTime,
                progWinAmt = @this.WinAmount,
                progWinText = @this.WinText,
                progWinSeq = @this.WinSequence,
                payMethod = (t_progPayMethods)@this.PayMethod,
                progPaidAmt = @this.PaidAmount,
                progException = @this.Exception,
                paidDateTime = @this.PaidDateTime
            };
        }

        /// <summary>
        ///     to get level type based on turnover
        /// </summary>
        /// <param name="this">level type</param>
        /// <param name="progressiveTurnover">turnover value</param>
        /// <returns></returns>
        public static string ToLevelType(this string @this, long progressiveTurnover)
        {
            if (progressiveTurnover > 0)
            {
                switch (@this)
                {
                    case "ATI_anteWithBulk":
                    case "ATI_mystery":
                    case "ATI_hostChoice":
                    {
                        return @this;
                    }
                }
            }

            return "ATI_STANDARD";
        }

        /// <summary>
        ///     To convert ProgressiveState type to t_progStates
        /// </summary>
        /// <param name="this">ProgressiveState</param>
        /// <returns>t_progStates</returns>
        public static t_progStates ToProgState(this ProgressiveState @this)
        {
            switch (@this)
            {
                case ProgressiveState.Acknowledged:
                    return t_progStates.G2S_progAck;
                case ProgressiveState.Committed:
                    return t_progStates.G2S_progCommit;
                case ProgressiveState.Failed:
                    return t_progStates.G2S_progFailed;
                case ProgressiveState.Hit:
                    return t_progStates.G2S_progHit;
                case ProgressiveState.Pending:
                    return t_progStates.G2S_progPend;
                default:
                    return t_progStates.G2S_progFailed;
            }
        }
    }
}