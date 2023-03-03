namespace Aristocrat.Monaco.Gaming.Consumers
{
    using System;
    using Accounting.Contracts;
    using Accounting.Contracts.Transactions;
    using Contracts;
    using Contracts.Barkeeper;
    using Hardware.Contracts.Persistence;

    /// <summary>
    /// The keyedCreditOnEvent Consumer Consumes KeyedCreditOnEvent 
    /// </summary>
    public class KeyedCreditOnConsumer : Consumes<KeyedCreditOnEvent>
    {
        private readonly ICurrencyInContainer _currencyHandler;
        private readonly ISessionInfoService _sessionInfoService;
        private readonly IPersistentStorageManager _persistentStorage;
        private readonly IBarkeeperHandler _barkeeperHandler;

        /// <summary>
        /// Initializes a new instance of the KeyedCreditOnConsumer class.
        /// </summary>
        /// <param name="currencyHandler"></param>
        /// <param name="sessionInfoService"></param>
        /// <param name="persistentStorage"></param>
        /// <param name="barkeeperHandler"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public KeyedCreditOnConsumer(
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

        /// <summary>
        /// The Consumer Method
        /// </summary>
        /// <param name="event"></param>
        public override void Consume(KeyedCreditOnEvent @event)
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
