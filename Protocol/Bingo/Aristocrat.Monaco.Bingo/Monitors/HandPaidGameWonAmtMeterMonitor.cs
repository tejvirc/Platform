namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Transactions;
    using Accounting.Contracts.TransferOut;
    using Application.Contracts;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class HandPaidGameWonAmtMeterMonitor : BaseGameWinAmountMeterMonitor
    {
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Creates an instance of <see cref="HandPaidGameWonAmtMeterMonitor"/>
        /// </summary>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="bingoGameProvider">An instance of <see cref="IBingoGameProvider"/></param>
        /// <param name="transactionQueue">An instance of <see cref="IReportTransactionQueueService"/></param>
        /// <param name="transactionHistory">An instance of <see cref="ITransactionHistory"/></param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        /// <param name="eventBus">An instance of <see cref="IEventBus"/></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameHistory"/>, <paramref name="bingoGameProvider"/>, <paramref name="transactionQueue"/>, or <paramref name="gameHistory"/></exception>
        public HandPaidGameWonAmtMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            ITransactionHistory transactionHistory,
            IGameHistory gameHistory,
            IEventBus eventBus)
            : base(
                GamingMeters.HandPaidGameWonAmount,
                false,
                meterManager,
                bingoGameProvider,
                transactionQueue,
                gameHistory,
                eventBus)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        protected override IEnumerable<HandpayTransaction> GetHandpays(IGameHistoryLog log)
        {
            return _transactionHistory.RecallTransactions<HandpayTransaction>()
                .Where(t => !t.IsCreditType() && IsForGameRound(t));

            bool IsForGameRound(ITransactionContext handpayTransaction) =>
                log.CashOutInfo.Any(
                    c => c.TraceId == handpayTransaction.TraceId && c.Reason is TransferOutReason.LargeWin &&
                         c.Amount == handpayTransaction.TransactionAmount);
        }
    }
}