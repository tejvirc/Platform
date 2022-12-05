namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Services.Reporting;
    using TransactionType = Common.TransactionType;

    public sealed class EgmPaidGameWinBonusAmountMeterMonitor : BaseCurrencyMeterMonitor
    {
        private readonly IBingoGameProvider _bingoGameProvider;
        private readonly IReportTransactionQueueService _transactionQueue;
        private readonly IBonusHandler _bonusHandler;
        private readonly IGameHistory _gameHistory;
        private readonly ITransactionHistory _transactionHistory;

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
            ITransactionHistory transactionHistory)
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