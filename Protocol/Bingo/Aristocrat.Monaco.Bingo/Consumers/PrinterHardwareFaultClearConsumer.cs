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
    ///     Handles the <see cref="HardwareFaultClearEvent" /> event for printers.
    /// </summary>
    public class PrinterHardwareFaultClearConsumer : AsyncConsumes<HardwareFaultClearEvent>
    {
        private readonly IReportEventQueueService _reportingService;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public PrinterHardwareFaultClearConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(HardwareFaultClearEvent theEvent, CancellationToken token)
        {
            switch (theEvent.Fault)
            {
                case PrinterFaultTypes.PaperEmpty:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterPaperOutClear);
                    await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
                    break;
                case PrinterFaultTypes.PaperNotTopOfForm:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterMediaLoaded);
                    break;
                default:
                    _reportingService.AddNewEventToQueue(ReportableEvent.PrinterErrorClear);
                    break;
            }
        }
    }
}