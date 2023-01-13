namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Application.Contracts;
    using Common;
    using Common.Storage;
    using Services.Reporting;

    /// <summary>
    ///     A base class for handler lifetime meter monitors
    /// </summary>
    public abstract class BaseLifetimeMeterMonitor : BaseMeterMonitor
    {
        /// <inheritdoc />
        protected BaseLifetimeMeterMonitor(
            string meterId,
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            TransactionType transactionType,
            IReportTransactionQueueService transactionQueue)
            : base(meterId, meterManager, bingoGameProvider, transactionType, transactionQueue)
        {
        }

        /// <inheritdoc />
        protected override long GetAmount(
            MeterChangedEventArgs changedEventArgs,
            BingoGameDescription gameDescription) => Meter.Lifetime;
    }
}