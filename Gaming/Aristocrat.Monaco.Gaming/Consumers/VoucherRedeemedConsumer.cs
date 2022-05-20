namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Barkeeper;

    public class VoucherRedeemedConsumer : Consumes<VoucherRedeemedEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IBarkeeperHandler _barkeeperHandler;

        public VoucherRedeemedConsumer(ICurrencyInContainer currencyHandler, IBarkeeperHandler barkeeperHandler)
        {
            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
        }

        public override void Consume(VoucherRedeemedEvent @event)
        {
            _currencyHandler.Credit(@event.Transaction);
            _barkeeperHandler.OnCreditsInserted(@event.Transaction.Amount);
        }
    }
}
