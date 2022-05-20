namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="PersistentStorageIntegrityCheckFailedEvent" /> event.
    /// </summary>
    public class PersistentStorageIntegrityCheckFailedConsumer : Consumes<PersistentStorageIntegrityCheckFailedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public PersistentStorageIntegrityCheckFailedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(PersistentStorageIntegrityCheckFailedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.NvRamError);
        }
    }
}