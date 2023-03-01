namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Services.Reporting;
    using TransactionType = Common.TransactionType;

    public sealed class EgmPaidGameWinBonusAmountMeterMonitor : BaseCurrencyMeterMonitor
    {
        private readonly IBingoGameProvider _bingoGameProvider;
        private readonly IReportTransactionQueueService _transactionQueue;
        private readonly IBonusHandler _bonusHandler;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IEventBus _eventBus;

        private bool _disposed;

        /// <summary>
        ///     Creates an instance of <see cref="EgmPaidGameWinBonusAmountMeterMonitor"/>
        /// </summary>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="bingoGameProvider">An instance of <see cref="IBingoGameProvider"/></param>
        /// <param name="transactionQueue">An instance of <see cref="IReportTransactionQueueService"/></param>
        /// <param name="bonusHandler">An instance of <see cref="IBonusHandler"/></param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        /// <param name="transactionHistory"></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameHistory"/>, <paramref name="bingoGameProvider"/>, <paramref name="transactionQueue"/>, or <paramref name="gameHistory"/></exception>
        public EgmPaidGameWinBonusAmountMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            IBonusHandler bonusHandler,
            IGameHistory gameHistory,
            ITransactionHistory transactionHistory,
            IEventBus eventBus)
            : base(
                BonusMeters.EgmPaidGameWinBonusAmount,
                meterManager,
                bingoGameProvider,
                TransactionType.CashWon,
                transactionQueue)
        {
            _bingoGameProvider = bingoGameProvider ?? throw new ArgumentNullException(nameof(bingoGameProvider));
            _transactionQueue = transactionQueue ?? throw new ArgumentNullException(nameof(transactionQueue));
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<GameAddedEvent>(this, _ => OnMeterChanged());
            _eventBus.Subscribe<GameRemovedEvent>(this, _ => OnMeterChanged());
        }

        /// <inheritdoc />
        protected override void ReportTransaction(MeterChangedEventArgs changedEventArgs)
        {
            var log = _gameHistory.CurrentLog;
            var bingoGame = _bingoGameProvider.GetBingoGame();
            if (log is null || bingoGame is null || changedEventArgs.Amount <= 0)
            {
                return;
            }

            HandleGameWins(changedEventArgs, log, bingoGame);
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
            base.Dispose(disposing);
        }

        private void HandleGameWins(
            MeterChangedEventArgs changedEventArgs,
            IGameHistoryLog log,
            BingoGameDescription bingoGame)
        {
            _transactionQueue.ReportEgmPaidTransactions(
                GetCreditHandpays(log),
                changedEventArgs.Amount,
                GetTitleId(changedEventArgs, bingoGame),
                GetDenominationId(changedEventArgs, bingoGame),
                GetGameSerial(changedEventArgs, bingoGame),
                GetPaytableId(changedEventArgs, bingoGame));
        }

        private IEnumerable<HandpayTransaction> GetCreditHandpays(IGameHistoryLog log)
        {
            var bonusTransactions = _bonusHandler.Transactions.Where(t => t.AssociatedTransactions.Contains(log.TransactionId)).ToList();
            foreach (var handpayTransaction in _transactionHistory.RecallTransactions<HandpayTransaction>())
            {
                if (!handpayTransaction.IsCreditType() ||
                    !bonusTransactions.Any(t => handpayTransaction.AssociatedTransactions.Contains(t.TransactionId)))
                {
                    continue;
                }

                yield return handpayTransaction;
            }
        }
    }
}