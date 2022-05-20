namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultClearEvent" /> event.
    ///     This event is sent by devices when a fault condition is cleared
    /// </summary>
    public class IdReaderHardwareFaultClearConsumer : Consumes<HardwareFaultClearEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public IdReaderHardwareFaultClearConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(HardwareFaultClearEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardReaderErrorClear);
        }
    }
}