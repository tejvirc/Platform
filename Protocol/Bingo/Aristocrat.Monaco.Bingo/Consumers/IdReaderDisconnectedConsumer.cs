namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="DisconnectedEvent" /> event.
    /// </summary>
    public class IdReaderDisconnectedConsumer : Consumes<DisconnectedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public IdReaderDisconnectedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(DisconnectedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardReaderCommunicationError);
        }
    }
}