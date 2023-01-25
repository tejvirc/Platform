namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Application.Contracts;
    using Common;
    using Gaming.Contracts;
    using Services.Reporting;

    /// <summary>
    ///     A meter monitor for wager amounts
    /// </summary>
    public sealed class WageredAmountMeterMonitor : BaseCurrencyMeterMonitor
    {
        /// <inheritdoc />
        public WageredAmountMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue)
            : base(
                GamingMeters.WageredAmount,
                meterManager,
                bingoGameProvider,
                TransactionType.CashPlayed,
                transactionQueue)
        {
        }
    }
}