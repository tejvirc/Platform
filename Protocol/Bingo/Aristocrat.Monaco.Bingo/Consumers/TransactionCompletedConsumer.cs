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
        private readonly ICommandHandlerFactory _commandFactory;

        public TransactionCompletedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            ICommandHandlerFactory handler)
            : base(eventBus, sharedConsumer)
        {
            _commandFactory = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <inheritdoc />
        public override async Task Consume(TransactionCompletedEvent _, CancellationToken token)
        {
            await _commandFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}
