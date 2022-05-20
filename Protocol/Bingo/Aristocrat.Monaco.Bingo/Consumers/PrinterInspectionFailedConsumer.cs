namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="InspectionFailedEvent" /> event.
    ///     This event is sent by the PrinterAdapter when
    ///     the inspection fails to find the expected printer.
    /// </summary>
    public class PrinterInspectionFailedConsumer : Consumes<InspectionFailedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public PrinterInspectionFailedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(InspectionFailedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.PrinterCommunicationError);
        }
    }
}