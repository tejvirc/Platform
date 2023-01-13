namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Common;
    using Common.Storage;
    using Services.Reporting;

    /// <summary>
    ///     A base class for handler currency meter monitors
    /// </summary>
    public abstract class BaseCurrencyMeterMonitor : BaseMeterMonitor
    {
        /// <inheritdoc />
        protected BaseCurrencyMeterMonitor(
            string meterId,
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            TransactionType transactionType,
            IReportTransactionQueueService transactionQueue)
            : base(meterId, meterManager, bingoGameProvider, transactionType, transactionQueue)
        {
        }

        /// <inheritdoc />
        protected override long GetAmount(MeterChangedEventArgs changedEventArgs, BingoGameDescription gameDescription) =>
            changedEventArgs.Amount.MillicentsToCents();
    }
}