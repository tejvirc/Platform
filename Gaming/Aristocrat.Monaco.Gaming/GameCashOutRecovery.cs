namespace Aristocrat.Monaco.Gaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Consumers;
    using Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     This class is used to recover cash outs that occur during a game.
    /// </summary>
    public class GameCashOutRecovery : IGameCashOutRecovery
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IPlayerBank _bank;
        private readonly IGameHistory _history;
        private readonly ITransactionHistory _transactionHistory;
        private readonly ICurrencyInContainer _currency;
        private readonly ITransferOutHandler _transferOutHandler;
        private readonly bool _meterFreeGames;

        public GameCashOutRecovery(
            IPlayerBank bank,
            IGameHistory gameHistory,
            ITransferOutHandler transferOutHandler,
            ITransactionHistory transactionHistory,
            ICurrencyInContainer currency,
            IPropertiesManager properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            _bank = bank ?? throw new ArgumentNullException(nameof(bank));
            _history = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _transferOutHandler = transferOutHandler ?? throw new ArgumentNullException(nameof(transferOutHandler));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _currency = currency ?? throw new ArgumentNullException(nameof(currency));
            _meterFreeGames = properties.GetValue(GamingConstants.MeterFreeGamesIndependently, false);
            RecoverTransactionHistory();
        }

        /// <inheritdoc />
        public bool HasPending => _history.HasPendingCashOut;

        /// <inheritdoc />
        public bool Recover()
        {
            var log = _history.CurrentLog;
            if (log is null || !HasPending)
            {
                return false;
            }

            // This will run asynchronously in the background.  We need to exclude it from the check below
            var recoveredIds = _transferOutHandler.Recover();
            var cashOutInfo = log.CashOutInfo.Where(i => !i.Complete).ToList();
            return cashOutInfo.Any() && RecoverCashout(
                log,
                recoveredIds,
                cashOutInfo,
                _transactionHistory.RecallTransactions().OfType<ITransactionContext>().ToList());
        }

        private void RecoverTransactionHistory()
        {
            var log = _history.CurrentLog;
            if (log is null || HasPending)
            {
                return;
            }

            ProcessTransactions(
                _transactionHistory.RecallTransactions(true)
                    .Select(t => (transaction: t, amount: (t is ITransactionContext context) ? context.TransactionAmount : 0))
                    .Where(x => x.amount > 0)
                    .TakeWhile(
                        x => x.transaction.TransactionId > log.TransactionId &&
                             log.Transactions.All(t => t.TransactionId != x.transaction.TransactionId)));
        }

        private bool RecoverCashout(
            IGameHistoryLog log,
            IReadOnlyCollection<Guid> recoveredIds,
            IEnumerable<CashOutInfo> cashOutInfo,
            IReadOnlyCollection<ITransactionContext> transactions)
        {
            var recovering = false;

            foreach (var cashOut in cashOutInfo)
            {
                if (recoveredIds.Contains(cashOut.TraceId))
                {
                    recovering = true;
                    continue;
                }

                var total = cashOut.Amount;
                var transactionContexts = transactions.Where(t => t.TraceId == cashOut.TraceId).ToList();
                var transactionTotal = transactionContexts.Sum(t => t.TransactionAmount);
                var transactionDetails = transactionContexts
                    .Select(t => (transaction: t as ITransaction, amount: t.TransactionAmount))
                    .Where(
                        x => x.transaction != null && x.amount > 0 &&
                             log.Transactions.All(t => t.TransactionId != x.transaction.TransactionId));
                ProcessTransactions(transactionDetails);
                if (total == transactionTotal)
                {
                    _history.CompleteCashOut(cashOut.TraceId);
                    Logger.Info($"Recovering with full transferred out amount.  Cashout amount={total}");
                    continue;
                }

                HandleCashout(log, transactionTotal, total, cashOut);
                recovering = true;
            }

            return recovering;
        }

        private void ProcessTransactions(IEnumerable<(ITransaction, long)> transactionDetails)
        {
            foreach (var (transaction, amount) in transactionDetails)
            {
                Logger.Debug(
                    $"Recovering the transaction({transaction}) into the game history that was completed but not saved to the game history");
                transaction.HandleOutTransaction(
                    amount,
                    _currency,
                    _history,
                    _transactionHistory,
                    _meterFreeGames);
            }
        }

        private void HandleCashout(IGameHistoryLog log, long transactionTotal, long total, CashOutInfo cashOut)
        {
            if (transactionTotal > 0)
            {
                Logger.Info(
                    $"Recovering with partial transferred out amount.  Expected Total={total} and found Total={transactionTotal}");
                total -= transactionTotal;
            }

            if (cashOut.Handpay)
            {
                Logger.Info(
                    $"Recovering handpay cashout TraceId={cashOut.TraceId}, Amount={cashOut.Amount}, Reason={cashOut.Reason}, TransactionId={log.TransactionId}");
                _bank.ForceHandpay(cashOut.TraceId, total, cashOut.Reason, log.TransactionId);
            }
            else
            {
                Logger.Info(
                    $"Recovering cashout TraceId={cashOut.TraceId}, Amount={cashOut.Amount}, Reason={cashOut.Reason}, TransactionId={log.TransactionId}");
                _bank.CashOut(cashOut.TraceId, total, cashOut.Reason, true, log.TransactionId);
            }
        }
    }
}
