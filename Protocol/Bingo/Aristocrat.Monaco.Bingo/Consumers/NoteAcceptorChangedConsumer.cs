namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="NoteAcceptorChangedEvent" /> event.
    ///     This event is sent by the NoteAcceptorAdapter when
    ///     the inspection indicates a different note acceptor
    ///     is present compared to the last inspection.
    /// </summary>
    public class NoteAcceptorChangedConsumer : Consumes<NoteAcceptorChangedEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public NoteAcceptorChangedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService =
                reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(NoteAcceptorChangedEvent theEvent)
        {
            _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorConfigurationError);
        }
    }
}