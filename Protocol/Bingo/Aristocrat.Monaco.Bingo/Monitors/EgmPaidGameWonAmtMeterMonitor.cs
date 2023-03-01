﻿namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Common.Storage;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;
    using TransactionType = Common.TransactionType;

    /// <summary>
    ///     A meter monitor for total egm paid
    /// </summary>
    public sealed class EgmPaidGameWonAmtMeterMonitor : BaseCurrencyMeterMonitor
    {
        private readonly IBingoGameProvider _bingoGameProvider;
        private readonly IReportTransactionQueueService _transactionQueue;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IGameHistory _gameHistory;
        private readonly IEventBus _eventBus;

        private bool _disposed;

        /// <summary>
        ///     Creates an instance of <see cref="EgmPaidGameWonAmtMeterMonitor"/>
        /// </summary>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="bingoGameProvider">An instance of <see cref="IBingoGameProvider"/></param>
        /// <param name="transactionQueue">An instance of <see cref="IReportTransactionQueueService"/></param>
        /// <param name="transactionHistory">An instance of <see cref="ITransactionHistory"/></param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameHistory"/>, <paramref name="bingoGameProvider"/>, <paramref name="transactionQueue"/>, or <paramref name="gameHistory"/></exception>
        public EgmPaidGameWonAmtMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            ITransactionHistory transactionHistory,
            IGameHistory gameHistory,
            IEventBus eventBus)
            : base(
                GamingMeters.EgmPaidGameWonAmount,
                meterManager,
                bingoGameProvider,
                TransactionType.CashWon,
                transactionQueue)
        {
            _bingoGameProvider = bingoGameProvider ?? throw new ArgumentNullException(nameof(bingoGameProvider));
            _transactionQueue = transactionQueue ?? throw new ArgumentNullException(nameof(transactionQueue));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
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
            return _transactionHistory.RecallTransactions<HandpayTransaction>()
                .Where(t => t.IsCreditType() && IsForGameRound(t));

            bool IsForGameRound(ITransactionContext handpayTransaction) =>
                log.CashOutInfo.Any(
                    c => c.TraceId == handpayTransaction.TraceId && c.Reason is TransferOutReason.LargeWin &&
                         c.Amount == handpayTransaction.TransactionAmount);
        }
    }
}