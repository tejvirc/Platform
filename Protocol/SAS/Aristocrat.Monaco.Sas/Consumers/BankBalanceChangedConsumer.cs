namespace Aristocrat.Monaco.Sas.Consumers
{
    using System;
    using Accounting.Contracts;
    using AftTransferProvider;
    using Contracts.Client;
    using Ticketing;

    /// <summary>
    ///     Handles the <see cref="BankBalanceChangedEvent" /> event.
    /// </summary>
    public class BankBalanceChangedConsumer : Consumes<BankBalanceChangedEvent>
    {
        private readonly IBank _bank;
        private readonly ITicketingCoordinator _ticketingCoordinator;
        private readonly AftTransferProviderBase _aftOffTransferProvider;
        private readonly AftTransferProviderBase _aftOnTransferProvider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BankBalanceChangedConsumer" /> class.
        /// </summary>
        /// <param name="bank">The bank</param>
        /// <param name="aftOffTransferProvider">The aft off transfer provider</param>
        /// <param name="aftOnTransferProvider">The aft on transfer provider</param>
        /// <param name="ticketingCoordinator">The ticketing coordinator</param>
        public BankBalanceChangedConsumer(
            IBank bank,
            IAftOffTransferProvider aftOffTransferProvider,
            IAftOnTransferProvider aftOnTransferProvider,
            ITicketingCoordinator ticketingCoordinator)
        {
            _bank = bank;
            _ticketingCoordinator = ticketingCoordinator ?? throw new ArgumentNullException(nameof(ticketingCoordinator));
            _aftOffTransferProvider = aftOffTransferProvider as AftTransferProviderBase;
            _aftOnTransferProvider = aftOnTransferProvider as AftTransferProviderBase;
        }

        /// <inheritdoc />
        public override void Consume(BankBalanceChangedEvent theEvent)
        {
            if (_bank.QueryBalance(AccountType.NonCash) == 0)
            {
                _ticketingCoordinator.RestrictedCreditsZeroed();
            }

            _aftOffTransferProvider.OnStateChanged();
            _aftOnTransferProvider.OnStateChanged();
        }
    }
}
