namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using Kernel.Contracts.Events;

    /// <summary>
    ///     Handles the <see cref="ConnectedEvent" /> event.
    /// </summary>
    public class NoteAcceptorConnectedConsumer : AsyncConsumes<ConnectedEvent>
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public NoteAcceptorConnectedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(ConnectedEvent theEvent, CancellationToken token)
        {
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}