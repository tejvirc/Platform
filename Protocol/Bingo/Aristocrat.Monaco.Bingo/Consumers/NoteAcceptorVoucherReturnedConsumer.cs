namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="VoucherReturnedEvent" /> event.
    /// </summary>
    public class NoteAcceptorVoucherReturnedConsumer : Consumes<VoucherReturnedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public NoteAcceptorVoucherReturnedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(VoucherReturnedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorDocumentReturned);
        }
    }
}