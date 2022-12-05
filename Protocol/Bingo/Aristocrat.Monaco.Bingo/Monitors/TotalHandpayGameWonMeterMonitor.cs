namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts;
    using Common.Storage;
    using Services.Reporting;
    using TransactionType = Common.TransactionType;

    /// <summary>
    ///     A meter monitor for total hand pay game won amount
    /// </summary>
    public sealed class TotalHandpayGameWonMeterMonitor : BaseCurrencyMeterMonitor
    {
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Creates an instance of <see cref="TotalHandpayGameWonMeterMonitor"/>
        /// </summary>
        /// <param name="meterManager">An instance of <see cref="IMeterManager"/></param>
        /// <param name="bingoGameProvider">An instance of <see cref="IBingoGameProvider"/></param>
        /// <param name="transactionQueue">An instance of <see cref="IReportTransactionQueueService"/></param>
        /// <param name="transactionHistory">An instance of <see cref="ITransactionHistory"/></param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="transactionHistory"/></exception>
        public TotalHandpayGameWonMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            ITransactionHistory transactionHistory)
            : base(
                AccountingMeters.TotalHandpaidGameWonAmount,
                meterManager,
                bingoGameProvider,
                TransactionType.LargeWin,
                transactionQueue)
        {
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        /// <inheritdoc />
        protected override string GetBarcode(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => _transactionHistory
            .RecallTransactions<HandpayTransaction>().OrderByDescending(t => t.TransactionId).FirstOrDefault()
            ?.Barcode ?? string.Empty;
    }
}