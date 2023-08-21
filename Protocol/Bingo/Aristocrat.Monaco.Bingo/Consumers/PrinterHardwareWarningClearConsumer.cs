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
    ///     Handles the <see cref="HardwareWarningClearEvent" /> event for printers.
    /// </summary>
    public class PrinterHardwareWarningClearConsumer : AsyncConsumes<HardwareWarningClearEvent>
    {
        private readonly IReportEventQueueService _reportingService;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public PrinterHardwareWarningClearConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(HardwareWarningClearEvent theEvent, CancellationToken token)
        {
            if (theEvent.Warning != PrinterWarningTypes.PaperLow)
            {
                return;
            }

            _reportingService.AddNewEventToQueue(ReportableEvent.PrinterPaperLowClear);
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}