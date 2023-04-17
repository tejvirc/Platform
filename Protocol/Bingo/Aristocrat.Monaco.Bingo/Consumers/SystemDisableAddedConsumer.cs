namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Kernel;
    using Kernel.Contracts.Events;

    public class SystemDisableAddedConsumer : AsyncConsumes<SystemDisableAddedEvent>
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public SystemDisableAddedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(SystemDisableAddedEvent _, CancellationToken token)
        {
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}