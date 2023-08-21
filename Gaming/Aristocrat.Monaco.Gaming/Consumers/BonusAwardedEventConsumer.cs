namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Contracts;
    using Contracts.Bonus;
    using Hardware.Contracts.Persistence;

    public class BonusAwardedEventConsumer : Consumes<BonusAwardedEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly IPersistentStorageManager _persistentStorage;

        public BonusAwardedEventConsumer(
            ICurrencyInContainer currencyHandler,
            IPersistentStorageManager persistentStorage)
        {
            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
        }

        public override void Consume(BonusAwardedEvent @event)
        {
            using var scope = _persistentStorage.ScopedTransaction();
            _currencyHandler.Credit(@event.Transaction);
            scope.Complete();
        }
    }
}