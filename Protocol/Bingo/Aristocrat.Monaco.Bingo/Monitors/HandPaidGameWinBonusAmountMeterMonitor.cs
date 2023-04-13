namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Bonus;
    using Kernel;
    using Services.Reporting;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class HandPaidGameWinBonusAmountMeterMonitor : BaseGameWinAmountMeterMonitor
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Creates an instance of <see cref="HandPaidGameWinBonusAmountMeterMonitor"/>
        /// </summary>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="bingoGameProvider">An instance of <see cref="IBingoGameProvider"/></param>
        /// <param name="transactionQueue">An instance of <see cref="IReportTransactionQueueService"/></param>
        /// <param name="bonusHandler">An instance of <see cref="IBonusHandler"/></param>
        /// <param name="gameHistory">An instance of <see cref="IGameHistory"/></param>
        /// <param name="transactionHistory">An instance of <see cref="ITransactionHistory"></param>
        /// <param name="eventBus">An instance of <see cref="IEventBus"/></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="gameHistory"/>, <paramref name="bingoGameProvider"/>, <paramref name="transactionQueue"/>, or <paramref name="gameHistory"/></exception>
        public HandPaidGameWinBonusAmountMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            IBonusHandler bonusHandler,
            IGameHistory gameHistory,
            ITransactionHistory transactionHistory,
            IEventBus eventBus)
            : base(
                BonusMeters.HandPaidGameWinBonusAmount,
                false,
                meterManager,
                bingoGameProvider,
                transactionQueue,
                gameHistory,
                eventBus)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        protected override IEnumerable<HandpayTransaction> GetHandpays(IGameHistoryLog log)
        {
            var bonusTransactions = _bonusHandler.Transactions.Where(t => t.AssociatedTransactions.Contains(log.TransactionId)).ToList();
            foreach (var handpayTransaction in _transactionHistory.RecallTransactions<HandpayTransaction>())
            {
                if (handpayTransaction.IsCreditType() ||
                    !bonusTransactions.Any(t => handpayTransaction.AssociatedTransactions.Contains(t.TransactionId)))
                {
                    continue;
                }

                yield return handpayTransaction;
            }
        }
    }
}