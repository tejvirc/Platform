namespace Aristocrat.Monaco.Bingo.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Commands;
    using Kernel;
    using Kernel.Contracts.Events;

    /// <summary>
    ///     Handles the <see cref="BankBalanceChangedEvent" /> event.
    /// </summary>
    public class BankBalanceChangedConsumer : AsyncConsumes<BankBalanceChangedEvent>
    {
        private readonly ICommandHandlerFactory _commandFactory;

        public BankBalanceChangedConsumer(
            IEventBus eventBus,
            ISharedConsumer sharedConsumer,
            ICommandHandlerFactory handler)
            : base(eventBus, sharedConsumer)
        {
            _commandFactory = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public override Task Consume(BankBalanceChangedEvent _, CancellationToken token)
        {
            return _commandFactory.Execute(new ReportEgmStatusCommand(), token);
        }
    }
}
