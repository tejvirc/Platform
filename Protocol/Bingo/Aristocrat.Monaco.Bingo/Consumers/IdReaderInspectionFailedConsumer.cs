namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="InspectionFailedEvent" /> event.
    /// </summary>
    public class IdReaderInspectionFailedConsumer : Consumes<InspectionFailedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public IdReaderInspectionFailedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(InspectionFailedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardReaderConfigurationError);
        }
    }
}