namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="InspectionFailedEvent" /> event.
    ///     This event is sent by the NoteAcceptorAdapter when
    ///     the inspection fails to find the expected note acceptor.
    /// </summary>
    public class NoteAcceptorInspectionFailedConsumer : Consumes<InspectionFailedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public NoteAcceptorInspectionFailedConsumer(
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
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorCommunicationsError);
        }
    }
}