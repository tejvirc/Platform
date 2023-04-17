namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Common;
    using Hardware.Contracts.Battery;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="BatteryLowEvent" /> event.
    /// </summary>
    public class BatteryLowConsumer : AsyncConsumes<BatteryLowEvent>
    {
        private readonly IReportEventQueueService _reportService;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public BatteryLowConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _reportService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _commandHandlerFactory =
                commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(BatteryLowEvent batteryLowEvent, CancellationToken token)
        {
            _reportService.AddNewEventToQueue(ReportableEvent.NvRamBatteryLow);
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}