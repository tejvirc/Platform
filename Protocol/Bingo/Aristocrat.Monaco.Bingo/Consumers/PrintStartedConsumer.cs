namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts.Events;

    public class PrintStartedConsumer : AsyncConsumes<PrintStartedEvent>
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public PrintStartedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(PrintStartedEvent theEvent, CancellationToken token)
        {
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}