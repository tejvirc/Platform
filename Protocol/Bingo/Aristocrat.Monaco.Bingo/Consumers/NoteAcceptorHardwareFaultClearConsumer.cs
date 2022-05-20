namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultClearEvent" /> event for note acceptors.
    /// </summary>
    public class NoteAcceptorHardwareFaultClearConsumer : Consumes<HardwareFaultClearEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public NoteAcceptorHardwareFaultClearConsumer(
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
            switch (theEvent.Fault)
            {
                case NoteAcceptorFaultTypes.StackerDisconnected:
                    _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.StackerInserted);
                    break;
                case NoteAcceptorFaultTypes.StackerFull:
                    _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorStackerFullClear);
                    break;
                default:
                    _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorErrorClear);
                    break;
            }
        }
    }
}