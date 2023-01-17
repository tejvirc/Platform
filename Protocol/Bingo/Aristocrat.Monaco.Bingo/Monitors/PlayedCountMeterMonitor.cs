namespace Aristocrat.Monaco.Bingo.Monitors
{
    using Application.Contracts;
    using Common;
    using Gaming.Contracts;
    using Services.Reporting;

    /// <summary>
    ///     A meter monitor for played count
    /// </summary>
    public sealed class PlayedCountMeterMonitor : BaseLifetimeMeterMonitor
    {
        /// <inheritdoc />
        public PlayedCountMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue)
            : base(
                GamingMeters.PlayedCount,
                meterManager,
                bingoGameProvider,
                TransactionType.GamesPlayed,
                transactionQueue)
        {
        }
    }
}