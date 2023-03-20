namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Collections.Generic;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Storage;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;

    /// <inheritdoc />
    public abstract class BaseGameWinAmountMeterMonitor : BaseMeterMonitor
    {
        private readonly IBingoGameProvider _bingoGameProvider;
        private readonly IReportTransactionQueueService _transactionQueue;
        private readonly IGameHistory _gameHistory;
        private readonly IEventBus _eventBus;

        private bool _disposed;

        /// <inheritdoc />
        protected BaseGameWinAmountMeterMonitor(
            string meterId,
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            IGameHistory gameHistory,
            IEventBus eventBus)
            : base(meterId, meterManager, bingoGameProvider, TransactionType.CashWon, transactionQueue)
        {
            _bingoGameProvider = bingoGameProvider ?? throw new ArgumentNullException(nameof(bingoGameProvider));
            _transactionQueue = transactionQueue ?? throw new ArgumentNullException(nameof(transactionQueue));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<GameAddedEvent>(this, _ => OnMeterChanged());
            _eventBus.Subscribe<GameRemovedEvent>(this, _ => OnMeterChanged());
        }

        /// <inheritdoc />
        protected override long GetAmount(MeterChangedEventArgs changedEventArgs, BingoGameDescription gameDescription) =>
            changedEventArgs?.Amount.MillicentsToCents() ?? throw new ArgumentNullException(nameof(changedEventArgs));

        /// <inheritdoc />
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

        /// <inheritdoc />
        protected override void ReportTransaction(MeterChangedEventArgs changedEventArgs)
        {
            var log = _gameHistory.CurrentLog;
            var bingoGame = _bingoGameProvider.GetBingoGame();
            if (changedEventArgs is null || log is null || bingoGame is null || changedEventArgs.Amount <= 0)
            {
                return;
            }

            HandleGameWins(changedEventArgs, log, bingoGame);
        }

        protected abstract IEnumerable<HandpayTransaction> GetCreditHandpays(IGameHistoryLog log);

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
    }
}