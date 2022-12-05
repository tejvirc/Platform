namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common.Storage;
    using Gaming.Contracts;
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
            IGameHistory gameHistory)
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

        private void HandleGameWins(
            MeterChangedEventArgs changedEventArgs,
            IGameHistoryLog log,
            BingoGameDescription bingoGame)
        {
            var creditHandpayTransactions = GetCreditHandpays(log);
            var gameTitleId = GetTitleId(changedEventArgs, bingoGame);
            var denominationId = GetDenominationId(changedEventArgs, bingoGame);
            var gameSerial = GetGameSerial(changedEventArgs, bingoGame);
            var paytableId = GetPaytableId(changedEventArgs, bingoGame);
            var handpayAmount = 0L;

            foreach (var handpayTransaction in creditHandpayTransactions)
            {
                var amount = handpayTransaction.TransactionAmount;
                _transactionQueue.AddNewTransactionToQueue(
                    TransactionType.LargeWin,
                    amount.MillicentsToCents(),
                    gameTitleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    handpayTransaction.Barcode);
                handpayAmount += amount;
            }

            var nonHandpaid = changedEventArgs.Amount - handpayAmount;
            if (nonHandpaid > 0)
            {
                _transactionQueue.AddNewTransactionToQueue(
                    TransactionType.CashWon,
                    nonHandpaid.MillicentsToCents(),
                    gameTitleId,
                    denominationId,
                    gameSerial,
                    paytableId,
                    string.Empty);
            }
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