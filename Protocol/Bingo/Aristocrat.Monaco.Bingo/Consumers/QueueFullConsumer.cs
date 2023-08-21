namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Common.Events;
    using Kernel;
    using Kernel.Contracts.Events;

    public class QueueFullConsumer : AsyncConsumes<QueueFullEvent>
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public QueueFullConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(QueueFullEvent theEvent, CancellationToken token)
        {
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}
