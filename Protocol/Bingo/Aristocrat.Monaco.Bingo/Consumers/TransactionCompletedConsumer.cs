namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Commands;
    using Kernel;
    using Kernel.Contracts.Events;

    /// <summary>
    ///     Handles the <see cref="TransactionCompletedEvent" /> event.
    /// </summary>
    public class TransactionCompletedConsumer : AsyncConsumes<TransactionCompletedEvent>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly ICommandHandlerFactory _commandFactory;

        public TransactionCompletedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            IPropertiesManager propertiesManager,
            ICommandHandlerFactory handler)
            : base(eventBus, sharedConsumer)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _commandFactory = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <inheritdoc />
        public override Task Consume(TransactionCompletedEvent _, CancellationToken token)
        {
            var serialNumber = _propertiesManager.GetValue(ApplicationConstants.SerialNumber, string.Empty);
            var response = new StatusResponseMessage(serialNumber);
            return _commandFactory.Execute(
                response,
                token);
        }
    }
}
