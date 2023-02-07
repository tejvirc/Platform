namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Commands;
    using Common;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.Reporting;

    public class SystemDisableRemovedConsumer : AsyncConsumes<SystemDisableRemovedEvent>
    {
        private readonly IReportEventQueueService _reportingService;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public SystemDisableRemovedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(SystemDisableRemovedEvent theEvent, CancellationToken token)
        {
            if (!theEvent.SystemDisabled)
            {
                _reportingService.AddNewEventToQueue(ReportableEvent.Enabled);
            }

            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}