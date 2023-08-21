namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Commands;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts.Events;

    public class PrintCompletedConsumer : AsyncConsumes<PrintCompletedEvent>
    {
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public PrintCompletedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(PrintCompletedEvent theEvent, CancellationToken token)
        {
            await _commandHandlerFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}