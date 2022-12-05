namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Application.Contracts;
    using Common;
    using Gaming.Contracts;
    using Services.Reporting;

    /// <summary>
    ///     A meter monitor for won count
    /// </summary>
    public sealed class WonCountMeterMonitor : BaseLifetimeMeterMonitor
    {
        /// <inheritdoc />
        public WonCountMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue)
            : base(
                GamingMeters.WonCount,
                meterManager,
                bingoGameProvider,
                TransactionType.GamesWon,
                transactionQueue)
        {
        }
    }
}