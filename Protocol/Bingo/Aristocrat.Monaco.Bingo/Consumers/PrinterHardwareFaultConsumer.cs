namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Common;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="HardwareFaultEvent" /> event for printers.
    /// </summary>
    public class PrinterHardwareFaultConsumer : AsyncConsumes<HardwareFaultEvent>
    {
        private readonly IReportEventQueueService _reportingService;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public PrinterHardwareFaultConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(HardwareFaultEvent theEvent, CancellationToken token)
        {
            switch (theEvent.Fault)
            {
                case PrinterFaultTypes.PrintHeadOpen:
                case PrinterFaultTypes.ChassisOpen:
                case PrinterFaultTypes.OtherFault:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterError);
                    break;
                case PrinterFaultTypes.TemperatureFault:
                case PrinterFaultTypes.PrintHeadDamaged:
                case PrinterFaultTypes.NvmFault:
                case PrinterFaultTypes.FirmwareFault:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterHardwareFailure);
                    break;
                case PrinterFaultTypes.PaperJam:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterMediaJam);
                    break;
                case PrinterFaultTypes.PaperEmpty:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterPaperOut);
                    await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
                    break;
                case PrinterFaultTypes.PaperNotTopOfForm:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterMediaMissingIndexMark);
                    break;
            }
        }
    }
}