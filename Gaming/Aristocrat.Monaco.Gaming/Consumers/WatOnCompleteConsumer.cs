namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.Persistence;

    /// <summary>
    ///     Consumer class for handling WatOnCompleteEvent.
    /// </summary>
    public class WatOnCompleteConsumer : Consumes<WatOnCompleteEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IBarkeeperHandler _barkeeperHandler;

        public WatOnCompleteConsumer(
            ICurrencyInContainer currencyHandler,
            ISessionInfoService sessionInfoService,
            IPersistentStorageManager persistentStorage,
            IBarkeeperHandler barkeeperHandler)
        {
            _currencyHandler = currencyHandler ?? throw new ArgumentNullException(nameof(currencyHandler));
            _sessionInfoService = sessionInfoService ?? throw new ArgumentNullException(nameof(sessionInfoService));
            _persistentStorage = persistentStorage ?? throw new ArgumentNullException(nameof(persistentStorage));
            _barkeeperHandler = barkeeperHandler ?? throw new ArgumentNullException(nameof(barkeeperHandler));
        }

        public override void Consume(WatOnCompleteEvent @event)
        {
            if (@event?.Transaction == null)
            {
                return;
            }

            var transaction = @event.Transaction;

            using (var scope = _persistentStorage.ScopedTransaction())
            {
                _currencyHandler.Credit(transaction);
                _barkeeperHandler.OnCreditsInserted(transaction.TransactionAmount);
                _sessionInfoService.HandleTransaction(transaction);
                scope.Complete();
            }
        }
    }
}