namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Commands;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts.Events;

    public class PrintStartedConsumer : AsyncConsumes<PrintStartedEvent>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly ICommandHandlerFactory _commandHandlerFactory;

        public PrintStartedConsumer(
            IEventBus eventBus,
            ISharedConsumer consumerContext,
            IPropertiesManager propertiesManager,
            ICommandHandlerFactory commandHandlerFactory)
            : base(eventBus, consumerContext)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _commandHandlerFactory = commandHandlerFactory ?? throw new ArgumentNullException(nameof(commandHandlerFactory));
        }

        public override async Task Consume(PrintStartedEvent theEvent, CancellationToken token)
        {
            var serialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            await _commandHandlerFactory.Execute(new StatusResponseMessage(serialNumber), token);
        }
    }
}