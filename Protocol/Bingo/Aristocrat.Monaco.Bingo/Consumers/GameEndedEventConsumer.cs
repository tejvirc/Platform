namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Commands;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Kernel.Contracts.Events;

    public class GameEndedEventConsumer : AsyncConsumes<GameEndedEvent>
    {
        private readonly ICommandHandlerFactory _commandHandler;
        private readonly ICentralProvider _centralProvider;
        private readonly IPropertiesManager _properties;

        public GameEndedEventConsumer(
            ICommandHandlerFactory commandHandler,
            ICentralProvider centralProvider,
            IPropertiesManager properties,
            IEventBus eventBus,
            ISharedConsumer context)
            : base(eventBus, context, evt => evt.Log.Result != GameResult.Failed)
        {
            _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
            _centralProvider = centralProvider ?? throw new ArgumentNullException(nameof(centralProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public override async Task Consume(GameEndedEvent theEvent, CancellationToken token)
        {
            var transaction = _centralProvider.Transactions.FirstOrDefault(
                x => x.AssociatedTransactions.Contains(theEvent.Log.TransactionId));
            if (transaction is null)
            {
                return;
            }

            var machineSerial = _properties.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            await _commandHandler.Execute(new BingoGameEndedCommand(machineSerial, transaction, theEvent.Log), token);
        }
    }
}
