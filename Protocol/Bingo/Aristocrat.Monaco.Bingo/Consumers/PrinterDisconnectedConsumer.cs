namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="DisconnectedEvent" /> event.
    /// </summary>
    public class PrinterDisconnectedConsumer : Consumes<DisconnectedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public PrinterDisconnectedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(DisconnectedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.PrinterCommunicationError);
        }
    }
}
