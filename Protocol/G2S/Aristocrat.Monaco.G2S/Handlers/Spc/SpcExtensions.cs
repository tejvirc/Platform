namespace Aristocrat.Monaco.G2S.Handlers.Spc
{
    using System;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Spc;

    public static class SpcExtensions
    {
        public static spcLog ToSpcLog(this SpcTransaction transaction, IGameProvider games)
        {
            var game = games.GetGame(transaction.GamePlayId);

            return new spcLog
            {
                transactionId = transaction.TransactionId,
                deviceId = transaction.DeviceId,
                logSequence = transaction.LogSequence,
                levelId = transaction.LevelId,
                gamePlayId = transaction.GamePlayId,
                themeId = game?.ThemeId,
                paytableId = game?.PaytableId,
                denomId = transaction.DenomId,
                winLevelIndex = transaction.WinLevelIndex,
                winLevelCombo = transaction.WinLevelCombo,
                spcState = transaction.State.ToSpcState(),
                hitAmt = transaction.HitAmount,
                paidAmt = transaction.PaidAmount,
                startupAmt = transaction.StartupAmount,
                overflowAmt = transaction.OverflowAmount,
                totalStartupAmt = transaction.TotalStartupAmount,
                totalAdjustAmt = transaction.TotalAdjustAmount,
                totalContribAmt = transaction.TotalContribAmount,
                totalHitAmt = transaction.TotalHitAmount,
                totalPaidAmt = transaction.TotalPaidAmount,
                spcDateTime = transaction.TransactionDateTime
            };
        }

        public static t_spcStates ToSpcState(this SpcState state)
        {
            switch (state)
            {
                case SpcState.Hit:
                    return t_spcStates.G2S_spcHit;
                case SpcState.Reset:
                    return t_spcStates.G2S_spcReset;
                case SpcState.Ack:
                    return t_spcStates.G2S_spcAck;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
