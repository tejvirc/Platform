namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using Common;
    using Kernel;
    using Monaco.Common;
    using Services.GamePlay;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="PlatformBootedEvent" /> event.
    /// </summary>
    public class PlatformBootedConsumer : Consumes<PlatformBootedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;
        private readonly IBingoReplayRecovery _replayRecovery;

        public PlatformBootedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            IBingoReplayRecovery replayRecovery)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _replayRecovery = replayRecovery ?? throw new ArgumentNullException(nameof(replayRecovery));
        }

        public override void Consume(PlatformBootedEvent e)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.PowerUp);

            if (e.CriticalMemoryCleared)
            {
                _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.NvRamCleared);
            }

            _replayRecovery.RecoverGamePlay(CancellationToken.None).FireAndForget();
        }
    }
}