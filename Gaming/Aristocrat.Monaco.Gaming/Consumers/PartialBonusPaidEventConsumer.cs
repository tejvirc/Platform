namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Application.Contracts;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts.Persistence;

    public class PartialBonusPaidEventConsumer : Consumes<PartialBonusPaidEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IIdProvider _idProvider;

        public PartialBonusPaidEventConsumer(
            ICurrencyInContainer currencyHandler,
            IPersistentStorageManager persistentStorage,
            IIdProvider idProvider)
        {
            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
        }

        public override void Consume(PartialBonusPaidEvent @event)
        {
            var paidAmount = @event.PaidCashable + @event.PaidNonCash + @event.PaidPromo;
            using var scope = _persistentStorage.ScopedTransaction();
            var transactionId = _idProvider.GetNextTransactionId();
            _currencyHandler.Credit(@event.Transaction, paidAmount, transactionId);
            scope.Complete();
        }
    }
}