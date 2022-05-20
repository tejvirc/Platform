namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters
{
    using System;
    using System.Linq;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Accounting.Contracts.TransferOut;
    using Aristocrat.Monaco.Accounting.Contracts.Wat;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Gaming.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;

    /// <summary>
    ///     Builds a meter snapshot model with values pertaining to a specific game round.
    /// </summary>
    public class TransactionMeterFactory
    {
        private readonly ICurrencyInContainer _currencyContainer;
        private readonly IBank _bank;

        public TransactionMeterFactory(
            ICurrencyInContainer currencyContainer,
            IBank bank)
        {
            _currencyContainer = currencyContainer;
            _bank = bank;
        }

        public GameMetersHistoryViewModel Build(
            IGameHistoryLog previousGame,
            IGameHistoryLog currentGame,
            IGameHistoryLog nextGame)
        {

            var result = new GameMetersHistoryViewModel();

            if (currentGame == null)
            {
                return result;
            }

            FillBeforeGameStartTransactions(
                currentGame,
                result.BeforeGameStart,
                previousGame
            );

            FillAfterGameEndTransactions(
                currentGame,
                result.AfterGameEnd
            );

            FillBeforeNextGameStartTransactions(
                currentGame,
                result.BeforeNextGame,
                nextGame
            );

            return result;
        }

        private void FillBeforeGameStartTransactions(IGameHistoryLog gameHistoryLog,
            GameRoundMeterSnapshotViewModel result, IGameHistoryLog previousGame)
        {
            var transactions = gameHistoryLog.Transactions
                .Where(t => t.TransactionId < gameHistoryLog.TransactionId);

            foreach (var transaction in transactions)
            {
                FillTransactionDetail(result, transaction);
            }

            result.Snapshot.CurrentCredits = gameHistoryLog.StartCredits;

            if (previousGame != null)
            {
                result.Snapshot.SecondaryPlayedCount = previousGame.SecondaryPlayed;
            }
        }

        private void FillAfterGameEndTransactions(IGameHistoryLog gameHistoryLog,
            GameRoundMeterSnapshotViewModel result)
        {
            var transactions = gameHistoryLog.Transactions
                .Where(t => t.TransactionId <= gameHistoryLog.EndTransactionId);

            foreach (var transaction in transactions)
            {
                FillTransactionDetail(result, transaction);
            }

            FillMachinePaidWinMeters(result, gameHistoryLog);
            FillAttendantPaidWinMeters(result, gameHistoryLog);
            FillWagerMeters(result.Snapshot, gameHistoryLog);
            FillGambleMeters(result.Snapshot, gameHistoryLog);

            result.Snapshot.CurrentCredits = gameHistoryLog.EndCredits;
            result.Snapshot.WageredAmount = gameHistoryLog.FinalWager.CentsToMillicents();
            result.Snapshot.SecondaryPlayedCount = gameHistoryLog.SecondaryPlayed;
        }

        private void FillBeforeNextGameStartTransactions(IGameHistoryLog gameHistoryLog,
            GameRoundMeterSnapshotViewModel result,
            IGameHistoryLog nextGameHistoryLog = null)
        {
            FillAfterGameEndTransactions(gameHistoryLog, result);

            if (nextGameHistoryLog == null)
            {
                foreach (var transaction in _currencyContainer.Transactions)
                {
                    FillTransactionDetail(result, transaction);
                }

                result.Snapshot.CurrentCredits = _bank.QueryBalance();
            }
            else
            {
                FillBeforeGameStartTransactions(nextGameHistoryLog, result, null);

                result.Snapshot.CurrentCredits = nextGameHistoryLog.StartCredits;
            }

            var postGameTransactions = gameHistoryLog.Transactions
                .Where(t => t.TransactionId > gameHistoryLog.EndTransactionId);

            foreach (var transaction in postGameTransactions)
            {
                FillTransactionDetail(result, transaction);
            }

            result.Snapshot.WageredAmount = gameHistoryLog.FinalWager.CentsToMillicents();
            result.Snapshot.SecondaryPlayedCount = gameHistoryLog.SecondaryPlayed;
        }

        private static void FillTransactionDetail
            (GameRoundMeterSnapshotViewModel detail,
            TransactionInfo transaction)
        {
            if (detail == null)
            {
                return;
            }

            // TODO: Resolve coin transactions if TrueCoinIn or TrueCoinOut are implemented
            detail.Snapshot.TrueCoinIn = 0;
            detail.Snapshot.TrueCoinOut = 0;
            
            if (transaction.TransactionType == typeof(BillTransaction))
            {
                detail.Snapshot.CurrencyInAmount += transaction.Amount;
            }
            else if (transaction.TransactionType == typeof(BonusTransaction))
            {
                if (transaction.HandpayType == HandpayType.BonusPay)
                {
                    detail.Snapshot.HandPaidBonusAmount += transaction.Amount;
                    detail.TotalPaidAmount += transaction.Amount;
                }
                else
                {
                    detail.Snapshot.HandPaidBonusAmount += transaction.Amount;
                }
            }
            else if (transaction.TransactionType == typeof(VoucherInTransaction))
            {
                detail.TotalVouchersIn += transaction.Amount;
                detail.Snapshot.VoucherInNonCashableAmount += transaction.NonCashablePromoAmount;
            }
            else if (transaction.TransactionType == typeof(VoucherOutTransaction))
            {
                detail.TotalVouchersOut += transaction.Amount;
                detail.Snapshot.VoucherOutNonCashableAmount += transaction.NonCashablePromoAmount;
            }
            else if (transaction.TransactionType == typeof(WatOnTransaction))
            {
                detail.WatOnTotalAmount += transaction.Amount;
                detail.Snapshot.WatOnNonCashableAmount += transaction.NonCashablePromoAmount;
                detail.Snapshot.WatOnCashablePromoAmount += transaction.CashablePromoAmount;
                detail.Snapshot.WatOnCashableAmount += transaction.CashableAmount;
            }
            else if (transaction.TransactionType == typeof(WatTransaction))
            {
                detail.WatOffTotalAmount += transaction.Amount;
                detail.Snapshot.WatOffNonCashableAmount += transaction.NonCashablePromoAmount;
                detail.Snapshot.WatOffCashablePromoAmount += transaction.CashablePromoAmount;
                detail.Snapshot.WatOffCashableAmount += transaction.CashableAmount;
            }
            else if (transaction.TransactionType == typeof(HandpayTransaction) &&
                     transaction.HandpayType == HandpayType.CancelCredit)
            {
                detail.Snapshot.HandpaidCancelAmount += transaction.Amount;
            }
        }

        private static void FillMachinePaidWinMeters(GameRoundMeterSnapshotViewModel meters,
            IGameHistoryLog gameHistoryLog)
        {
            if (gameHistoryLog.EndDateTime.Equals(DateTime.MinValue))
            {
                return;
            }

            var machinePaidWin = gameHistoryLog.TotalWon.CentsToMillicents() - GetAttendantPaidGameWin(gameHistoryLog);
            var progWin = !(gameHistoryLog.SecondaryPlayed > 0 && gameHistoryLog.Result == GameResult.Lost) ? gameHistoryLog.Jackpots.Sum(info => info.WinAmount) : 0;
            var handPaidProgWinFromJackpotInfo = GetHandPaidProgressiveWinInfo(gameHistoryLog);

            meters.Snapshot.EgmPaidProgWonAmount += progWin - handPaidProgWinFromJackpotInfo;
            meters.TotalEgmPaidAmount += machinePaidWin;
            meters.TotalPaidAmount += machinePaidWin;
        }

        private static void FillAttendantPaidWinMeters(GameRoundMeterSnapshotViewModel meters,
            IGameHistoryLog gameHistoryLog)
        {
            if (gameHistoryLog.EndDateTime.Equals(DateTime.MinValue))
            {
                return;
            }

            var gameWin = GetAttendantPaidGameWin(gameHistoryLog);
            var handPaidProgWinFromJackpotInfo = GetHandPaidProgressiveWinInfo(gameHistoryLog);

            meters.Snapshot.HandPaidProgWonAmount += handPaidProgWinFromJackpotInfo;
            meters.TotalHandPaidAmount += gameWin;
            meters.TotalPaidAmount += gameWin;
        }


        private static void FillWagerMeters(GameRoundMeterSnapshot meters, IGameHistoryLog gameHistoryLog)
        {
            if (gameHistoryLog.EndDateTime.Equals(DateTime.MinValue))
            {
                return;
            }

            var promoWager = gameHistoryLog.PromoWager;
            meters.WageredPromoAmount += promoWager;
        }

        private static void FillGambleMeters(GameRoundMeterSnapshot meters, IGameHistoryLog gameHistoryLog)
        {
            if (gameHistoryLog.EndDateTime.Equals(DateTime.MinValue))
            {
                return;
            }

            var gambleWagered = gameHistoryLog.SecondaryWager;
            var gambleWon = gameHistoryLog.SecondaryWin;

            meters.SecondaryWageredAmount += gambleWagered;
            meters.SecondaryWonAmount += gambleWon;
        }

        private static long GetAttendantPaidGameWin(IGameHistoryLog gameHistoryLog)
        {
            return (from cashoutInfo in
                    gameHistoryLog.CashOutInfo.Where(c => c.Handpay && c.Reason == TransferOutReason.LargeWin && c.Complete)
                    let transactionInfos =
                        gameHistoryLog.Transactions.Where(x => cashoutInfo.AssociatedTransactions.Contains(x.TransactionId))
                    where transactionInfos.All(
                        x => x.KeyOffType != KeyOffType.LocalCredit && x.KeyOffType != KeyOffType.RemoteCredit)
                    select cashoutInfo.Amount).Sum();
        }

        private static long GetHandPaidProgressiveWinInfo(IGameHistoryLog gameHistoryLog)
        {
            return gameHistoryLog.Jackpots.Where(
                       jackpot => gameHistoryLog.CashOutInfo.Any(
                           cashout => cashout.AssociatedTransactions.Contains(jackpot.TransactionId) &&
                                   cashout.Handpay && cashout.Reason != TransferOutReason.CashWin))
                   .Sum(info => info.WinAmount);
        }
    }
}
