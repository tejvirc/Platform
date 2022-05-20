namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="CurrencyReturnedEvent" /> event.
    /// </summary>
    public class NoteAcceptorCurrencyReturnedConsumer : Consumes<CurrencyReturnedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public NoteAcceptorCurrencyReturnedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(CurrencyReturnedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorDocumentReturned);
        }
    }
}