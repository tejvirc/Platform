namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="DocumentRejectedEvent" /> event.
    /// </summary>
    public class NoteAcceptorDocumentRejectedConsumer : Consumes<DocumentRejectedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public NoteAcceptorDocumentRejectedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(DocumentRejectedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorDocumentRejected);
        }
    }
}
