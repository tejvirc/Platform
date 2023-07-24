namespace Aristocrat.Monaco.Gaming.Proto.Wrappers
{
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Proto.Wrappers;
    using System;

    public static class GameHistoryLogExtension
    {
        public static Aristocrat.Monaco.Gaming.Proto.Wrappers.GameHistoryLogDecorator From(this Aristocrat.Monaco.Gaming.Proto.Wrappers.GameHistoryLogDecorator instance, IGameHistoryLog source)
        {
            return new Proto.Wrappers.GameHistoryLogDecorator
            {
                StorageIndex = source.StorageIndex,
                RecoveryBlob = source.RecoveryBlob,
                DenomConfiguration = source.DenomConfiguration,
                TransactionId = source.TransactionId,
                LogSequence = source.LogSequence,
                StartDateTime = source.StartDateTime.ToUniversalTime(),
                EndDateTime = source.EndDateTime.ToUniversalTime(),
                EndTransactionId = source.EndTransactionId,
                GameId = source.GameId,
                DenomId = source.DenomId,
                StartCredits = source.StartCredits,
                EndCredits = source.EndCredits,
                PlayState = source.PlayState,
                InitialWager = source.InitialWager,
                FinalWager = source.FinalWager,
                PromoWager = source.PromoWager,
                UncommittedWin = source.UncommittedWin,
                InitialWin = source.InitialWin,
                SecondaryPlayed = source.SecondaryPlayed,
                SecondaryWager = source.SecondaryWager,
                SecondaryWin = source.SecondaryWin,
                FinalWin = source.FinalWin,
                GameWinBonus = source.GameWinBonus,
                AmountOut = source.AmountOut,
                LastUpdate = source.LastUpdate.ToUniversalTime(),
                LastCommitIndex = source.LastCommitIndex,
                GameRoundDescriptions = source.GameRoundDescriptions,
                JackpotSnapshot = source.JackpotSnapshot,
                JackpotSnapshotEnd = source.JackpotSnapshotEnd,
                Jackpots = source.Jackpots,
                Transactions = source.Transactions,
                Events = source.Events,
                MeterSnapshots = source.MeterSnapshots,
                FreeGameIndex = source.FreeGameIndex,
                ErrorCode = source.ErrorCode,
                FreeGames = source.FreeGames,
                CashOutInfo = source.CashOutInfo,
                Outcomes = source.Outcomes,
                LocaleCode = source.LocaleCode,
                GameConfiguration = source.GameConfiguration,
                GameRoundDetails = source.GameRoundDetails,
            };
        }
    }
}
