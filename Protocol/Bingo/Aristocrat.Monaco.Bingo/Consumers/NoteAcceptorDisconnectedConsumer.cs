namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Common;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts.Events;
    using Services.Reporting;

    /// <summary>
    ///     Handles the <see cref="DisconnectedEvent" /> event.
    /// </summary>
    public class NoteAcceptorDisconnectedConsumer : AsyncConsumes<DisconnectedEvent>
    {
        private readonly IReportEventQueueService _reportingService;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public NoteAcceptorDisconnectedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IReportEventQueueService reportingService,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(DisconnectedEvent theEvent, CancellationToken token)
        {
            _reportingService.AddNewEventToQueue(ReportableEvent.BillAcceptorCommunicationsError);
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}
