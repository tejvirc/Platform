namespace Aristocrat.Monaco.Bingo.Monitors
{
    using System;
    using Application.Contracts;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     A meter monitor for played count
    /// </summary>
    public sealed class PlayedCountMeterMonitor : BaseLifetimeMeterMonitor
    {
        private readonly IEventBus _eventBus;

        private bool _disposed;

        /// <inheritdoc />
        public PlayedCountMeterMonitor(
            IMeterManager meterManager,
            IBingoGameProvider bingoGameProvider,
            IReportTransactionQueueService transactionQueue,
            IEventBus eventBus)
            : base(
                GamingMeters.PlayedCount,
                meterManager,
                bingoGameProvider,
                TransactionType.GamesPlayed,
                transactionQueue)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _eventBus.Subscribe<GameAddedEvent>(this, _ => OnMeterChanged());
            _eventBus.Subscribe<GameRemovedEvent>(this, _ => OnMeterChanged());
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
            base.Dispose(disposing);
        }
    }
}