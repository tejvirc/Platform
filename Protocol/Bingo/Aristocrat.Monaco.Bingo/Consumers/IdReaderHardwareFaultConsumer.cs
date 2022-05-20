namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using Common;
    using Hardware.Contracts.IdReader;
    using Kernel;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultEvent" /> event.
    ///     This event is sent by devices when a hardware fault is present
    /// </summary>
    public class IdReaderHardwareFaultConsumer : Consumes<HardwareFaultEvent>
    {
        private readonly IReportEventQueueService _bingoServerEventReportingService;

        public IdReaderHardwareFaultConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService)
            : base(eventBus, consumerContext)
        {
            _bingoServerEventReportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
        }

        public override void Consume(HardwareFaultEvent theEvent)
        {
            switch (theEvent.Fault)
            {
                case IdReaderFaultTypes.ComponentFault:
                    _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardReaderHardwareError);
                    break;
                case IdReaderFaultTypes.FirmwareFault:
                    _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardReaderSoftwareError);
                    break;
                default:
                    _bingoServerEventReportingService.AddNewEventToQueue(ReportableEvent.CardReaderError);
                    break;
            }
        }
    }
}